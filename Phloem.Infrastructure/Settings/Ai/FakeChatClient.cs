using System.Runtime.CompilerServices;
using System.Text.Json;
using Microsoft.Extensions.AI;
using Phloem.Domain.Constants;

namespace Phloem.Infrastructure.Settings.Ai;

/// <summary>
/// A pretend model (setting <c>Ai:Provider</c> = <c>Fake</c>): answers every job with its sample answer from
/// <see cref="AiPrompts"/>, at no cost. For tests and for trying Admin → AI before there's a key.
/// </summary>
internal sealed class FakeChatClient : IChatClient
{
    /// <summary>Where the AI client puts the job's key, so the fake knows which sample to return.</summary>
    public const string PromptKeyProperty = "phloem.promptKey";

    /// <inheritdoc />
    public Task<ChatResponse> GetResponseAsync(IEnumerable<ChatMessage> messages, ChatOptions? options = null,
                                               CancellationToken cancellationToken = default)
    {
        var key = options?.AdditionalProperties?.TryGetValue(PromptKeyProperty, out var value) == true
            ? value as string
            : null;
        var list = messages.ToList();
        var answer = key == AiPrompts.PromptRewrite
            ? RewriteAnswer(list.LastOrDefault(m => m.Role == ChatRole.User)?.Text)
            : (key is null ? null : AiPrompts.Find(key)?.SampleAnswer) ?? "{}";
        var sent = list.Sum(m => m.Text?.Length ?? 0);

        return Task.FromResult(new ChatResponse(new ChatMessage(ChatRole.Assistant, answer))
        {
            ModelId = "fake",
            Usage = new UsageDetails { InputTokenCount = sent / 4, OutputTokenCount = answer.Length / 4 }
        });
    }

    /// <summary>
    /// "Ask AI to improve this prompt" in test mode: gives back the wording it was sent, unchanged (so every answer
    /// field is still there), and says plainly that a real AI would have made the change.
    /// </summary>
    /// <param name="input">The JSON the rewrite job was sent (currentPrompt, request…).</param>
    private static string RewriteAnswer(string? input)
    {
        string current = "", request = "";
        try
        {
            using var doc = JsonDocument.Parse(input ?? "{}");
            if (doc.RootElement.TryGetProperty("currentPrompt", out var c)) current = c.GetString() ?? "";
            if (doc.RootElement.TryGetProperty("request", out var r)) request = r.GetString() ?? "";
        }
        catch (JsonException)
        {
            // Not the expected JSON: answer with an empty suggestion
        }

        return JsonSerializer.Serialize(new
        {
            prompt = current,
            summary = "Test mode (no AI key on this computer): the wording is returned unchanged. With the real AI, " +
                      $"this would explain what it changed for \"{request}\" and why."
        });
    }

    /// <inheritdoc />
    public async IAsyncEnumerable<ChatResponseUpdate> GetStreamingResponseAsync(
        IEnumerable<ChatMessage> messages, ChatOptions? options = null,
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        var response = await GetResponseAsync(messages, options, cancellationToken);
        foreach (var update in response.ToChatResponseUpdates()) yield return update;
    }

    /// <inheritdoc />
    public object? GetService(Type serviceType, object? serviceKey = null) =>
        serviceKey is null && serviceType.IsInstanceOfType(this) ? this : null;

    /// <inheritdoc />
    public void Dispose() { }
}
