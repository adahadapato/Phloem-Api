using System.Diagnostics;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.Caching.Hybrid;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Phloem.Domain.Constants;
using Phloem.Domain.Entities;
using Phloem.Domain.Enums;
using Phloem.Domain.Interfaces;
using Phloem.Domain.Models;
using Phloem.Domain.Shared.Errors;
using Phloem.Domain.Shared.Results;

// Phloem's order chat has its own ChatMessage / ChatRole; here they mean the AI's
using ChatMessage = Microsoft.Extensions.AI.ChatMessage;
using ChatRole = Microsoft.Extensions.AI.ChatRole;

namespace Phloem.Infrastructure.Settings.Ai;

/// <summary>Implements <see cref="IAiJsonClient"/>: one checked JSON answer per call, every call logged.</summary>
/// <remarks>
/// <para><b>Order:</b> AI on? → this month's spend under the cap? → current wording → ask (job's model, job's
/// answer length) → read the JSON object → required fields present? → log (own save) → answer.</para>
/// <para><b>Spend:</b> this month's total is cached for a minute, so a busy minute can go a few cents over the cap,
/// never more.</para>
/// </remarks>
internal sealed class AiJsonClient(
    AiModel model,
    IOptions<AiSettingsOptions> options,
    IAiPromptProvider prompts,
    HybridCache cache,
    IServiceScopeFactory scopes,
    TimeProvider time,
    ILogger<AiJsonClient> logger) : IAiJsonClient
{
    private const string AnswerRule =
        "\n\nAnswer with ONE JSON object only (no other text, no code fences).";

    private static readonly HybridCacheEntryOptions UsageCacheFor = new()
    {
        Expiration = TimeSpan.FromMinutes(1),
        LocalCacheExpiration = TimeSpan.FromMinutes(1)
    };

    /// <inheritdoc />
    public async Task<Result<AiJsonAnswer>> AskAsync(AiJsonRequest request, CancellationToken cancellationToken = default)
    {
        var o = options.Value;
        var definition = AiPrompts.Find(request.PromptKey)
            ?? throw new ArgumentException($"Unknown AI prompt '{request.PromptKey}'.", nameof(request));

        // 1. On, and under this month's cap?
        if (model.Client is null) return AiErrors.Disabled;
        if (await MonthCostAsync(cancellationToken) >= o.MonthlyCapUsd) return AiErrors.SpendCapReached;

        // 2. Ask
        var prompt = await prompts.GetAsync(definition.Key, cancellationToken);
        var modelName = o.Provider == AiProvider.Fake ? "fake" : o.ModelFor(definition.Key);
        var chatOptions = new ChatOptions
        {
            ModelId = modelName,
            MaxOutputTokens = definition.MaxOutputTokens,
            AdditionalProperties = new AdditionalPropertiesDictionary
            {
                [FakeChatClient.PromptKeyProperty] = definition.Key
            }
        };
        List<ChatMessage> messages =
        [
            new(ChatRole.System, prompt.Text + AnswerRule),
            new(ChatRole.User, request.Input)
        ];

        int firstTryInput = 0, firstTryOutput = 0;   // a cut-off first try still costs money
        var watch = Stopwatch.StartNew();
        ChatResponse? response = null;
        string? failure = null;
        try
        {
            response = await model.Client.GetResponseAsync(messages, chatOptions, cancellationToken);

            // Cut off because it ran out of room (newer models also spend room thinking): try once with double
            if (response.FinishReason == ChatFinishReason.Length && chatOptions.MaxOutputTokens is { } room)
            {
                logger.LogInformation("AI answer for {PromptKey} was cut off at {Tokens} tokens; trying again with more room.",
                                      definition.Key, room);
                var first = response;
                chatOptions.MaxOutputTokens = Math.Min(room * 2, 16_000);
                response = await model.Client.GetResponseAsync(messages, chatOptions, cancellationToken);
                firstTryInput = (int)(first.Usage?.InputTokenCount ?? 0);
                firstTryOutput = (int)(first.Usage?.OutputTokenCount ?? 0);
            }
        }
        catch (Exception ex) when (ex is not OperationCanceledException || !cancellationToken.IsCancellationRequested)
        {
            // Timeouts arrive as OperationCanceledException without our token being cancelled
            failure = $"{ex.GetType().Name}: {ex.Message}";
            logger.LogWarning(ex, "AI call for {PromptKey} failed.", definition.Key);
        }
        watch.Stop();

        // 3. Check the answer
        var reply = response?.Text;
        var inputTokens = (int)(response?.Usage?.InputTokenCount ?? 0) + firstTryInput;
        var outputTokens = (int)(response?.Usage?.OutputTokenCount ?? 0) + firstTryOutput;
        var cost = o.Provider == AiProvider.Fake ? 0m : o.CostOf(inputTokens, outputTokens);

        AiCallOutcome outcome;
        Error? error = null;
        System.Text.Json.JsonElement json = default;

        if (failure is not null)
        {
            outcome = AiCallOutcome.ProviderFailed;
            error = AiErrors.ProviderFailed;
        }
        else if (AiAnswerParser.ReadObject(reply) is not { } parsed)
        {
            outcome = AiCallOutcome.BadAnswer;
            failure = response?.FinishReason == ChatFinishReason.Length
                ? $"The answer was cut off (too long for {chatOptions.MaxOutputTokens} tokens)."
                : "The answer wasn't a JSON object.";
            error = AiErrors.BadAnswer;
        }
        else if (AiAnswerParser.MissingFields(parsed, definition.RequiredFields) is { Count: > 0 } missing)
        {
            outcome = AiCallOutcome.BadAnswer;
            failure = $"Missing fields: {string.Join(", ", missing)}.";
            error = AiErrors.BadAnswer;
        }
        else
        {
            outcome = AiCallOutcome.Succeeded;
            json = parsed;
        }

        // 4. Log (always), then answer
        var log = AiCallLog.Record(definition.Key, prompt.Version, request.Subject, o.Provider,
                                   response?.ModelId ?? modelName, request.Input, reply, outcome, failure,
                                   inputTokens, outputTokens, cost, watch.Elapsed);
        await SaveLogAsync(log);

        return error is null ? new AiJsonAnswer(log.Id, json) : error;
    }

    /// <summary>This calendar month's AI cost (cached for a minute).</summary>
    private async Task<decimal> MonthCostAsync(CancellationToken ct)
    {
        var now = time.GetUtcNow().UtcDateTime;
        var monthStart = new DateTime(now.Year, now.Month, 1, 0, 0, 0, DateTimeKind.Utc);
        try
        {
            return await cache.GetOrCreateAsync($"ai:cost:{now:yyyyMM}", async token =>
            {
                await using var scope = scopes.CreateAsyncScope();
                var calls = scope.ServiceProvider.GetRequiredService<IAiCallLogRepository>();
                return (await calls.GetUsageSinceAsync(monthStart, token)).CostUsd;
            }, UsageCacheFor, cancellationToken: ct);
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            // Can't check the spend: don't spend
            logger.LogWarning(ex, "Couldn't read this month's AI spend; treating the cap as reached.");
            return decimal.MaxValue;
        }
    }

    /// <summary>Saves the log row in its own scope, so the caller's unsaved changes are never saved with it.</summary>
    private async Task SaveLogAsync(AiCallLog log)
    {
        try
        {
            await using var scope = scopes.CreateAsyncScope();
            scope.ServiceProvider.GetRequiredService<IAiCallLogRepository>().Add(log);
            await scope.ServiceProvider.GetRequiredService<IUnitOfWork>().SaveChangesAsync(CancellationToken.None);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Couldn't save the AI call log for {PromptKey}.", log.PromptKey);
        }
    }
}
