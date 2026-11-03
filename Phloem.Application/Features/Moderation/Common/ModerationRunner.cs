using System.Text.Json;
using Microsoft.Extensions.Logging;
using Phloem.Domain.Constants;
using Phloem.Domain.Entities;
using Phloem.Domain.Enums;
using Phloem.Domain.Interfaces;
using Phloem.Domain.Models;
using Phloem.Domain.Shared.Errors;

namespace Phloem.Application.Features.Moderation.Common;

/// <summary>
/// The background checker's work (every minute): asks the AI about each waiting check and applies the answer.
/// </summary>
/// <remarks>
/// <list type="bullet">
///   <item><description><b>Dish:</b> OK → buyers see it. Needs a look / Not allowed → stays hidden, goes to the
///   Moderation queue.</description></item>
///   <item><description><b>Shop name:</b> the answer shows on the shop's review page; a doubtful name of a live shop
///   goes to the queue.</description></item>
///   <item><description><b>Dish name suggestion:</b> advice shown next to it on Admin → Dish names.</description></item>
///   <item><description><b>Contact message:</b> spam moves to the Spam tab; urgent and a summary show in the inbox.</description></item>
/// </list>
/// <para><b>When the AI can't check</b> (off, spend cap, or failed 3 times): dishes go to a person (decided
/// 3 Nov 2026), live shops' names too; advice-only checks finish unchecked.</para>
/// <para>"Hold everything" (<c>Moderation:HoldAll</c>) sends every dish and live shop name to a person, with the AI's
/// opinion attached. Dishes and shops that existed before checks began are checked once, a few each minute.</para>
/// </remarks>
public sealed class ModerationRunner(
    IModerationCheckRepository checks,
    IDishRepository dishes,
    IVendorRepository vendors,
    IDishCatalogRepository catalog,
    IContactMessageRepository messages,
    IAiJsonClient ai,
    IModerationSettingsProvider settings,
    IUnitOfWork unitOfWork,
    ILogger<ModerationRunner> logger)
{
    private const int BackfillPerRun = 10;

    /// <summary>Checks up to <paramref name="max"/> waiting items.</summary>
    /// <param name="max">Most checks in this run.</param>
    /// <param name="ct">Stops the run.</param>
    /// <returns>How many were finished (by the AI or sent to a person).</returns>
    public async Task<int> RunAsync(int max, CancellationToken ct)
    {
        await QueueOldContentAsync(ct);

        var done = 0;
        string? aiDown = null;   // once the AI is off or over its cap, don't ask again in this run

        foreach (var check in await checks.GetWaitingForAiAsync(max, ct))
        {
            try
            {
                aiDown = await ProcessAsync(check, aiDown, ct);
                await unitOfWork.SaveChangesAsync(ct);
                done++;
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                logger.LogError(ex, "Moderation check {CheckId} failed; it will be tried again.", check.Id);
            }
        }
        return done;
    }

    // ───────────────────────────── One check ─────────────────────────────

    /// <returns>Why the AI is unavailable for the rest of the run, or null.</returns>
    private async Task<string?> ProcessAsync(ModerationCheck check, string? aiDown, CancellationToken ct)
    {
        switch (check.Kind)
        {
            case ModerationKind.Dish:
            {
                var dish = await dishes.GetByIdAsync(check.TargetId, ct);
                if (dish is null || dish.ModerationText != check.Text) { check.Replace(); return aiDown; }

                var input = JsonSerializer.Serialize(new
                {
                    dishName = dish.Name,
                    extraDetail = dish.Detail,
                    description = dish.Description,
                    shop = dish.Vendor.Name,
                    country = CountryName(dish.Country)
                });
                return await AskVerdictAsync(check, AiPrompts.DishCheck, input, $"Dish {dish.Name}", aiDown,
                    needsPerson: v => v != ModerationVerdict.Ok || settings.HoldAll,
                    apply: toPerson => { if (toPerson) dish.SendTextToStaff(); else dish.ApproveText(); },
                    whenNotChecked: () => { dish.SendTextToStaff(); return true; }, ct);
            }

            case ModerationKind.ShopName:
            {
                var shop = await vendors.GetByIdAsync(check.TargetId, ct);
                if (shop is null || shop.Name != check.Text) { check.Replace(); return aiDown; }

                var input = JsonSerializer.Serialize(new { shopName = shop.Name, country = CountryName(shop.Country) });
                // New shops are approved by staff anyway (the answer shows on their review page); only live shops queue
                return await AskVerdictAsync(check, AiPrompts.ShopNameCheck, input, $"Shop {shop.Name}", aiDown,
                    needsPerson: v => shop.IsVisibleToBuyers && (v != ModerationVerdict.Ok || settings.HoldAll),
                    apply: _ => { },
                    whenNotChecked: () => shop.IsVisibleToBuyers, ct);
            }

            case ModerationKind.DishNameSuggestion:
            {
                var suggestion = await catalog.GetByIdAsync(check.TargetId, ct);
                if (suggestion is null) { check.Replace(); return aiDown; }

                var existing = (await catalog.GetApprovedAsync(ct))
                               .Where(c => c.Category == suggestion.Category).Select(c => c.Name).Take(300).ToList();
                var input = JsonSerializer.Serialize(new
                {
                    suggestedName = suggestion.Name,
                    category = suggestion.Category.ToString(),
                    existingNames = existing
                });
                return await AskVerdictAsync(check, AiPrompts.DishNameCheck, input, $"Dish name {suggestion.Name}", aiDown,
                    needsPerson: _ => false, apply: _ => { }, whenNotChecked: () => false, ct, labelField: "closeMatch");
            }

            case ModerationKind.ContactMessage:
            {
                var message = await messages.GetByIdAsync(check.TargetId, ct);
                if (message is null) { check.Replace(); return aiDown; }
                if (aiDown is not null) { check.NotCheckedByAi(aiDown, needsPerson: false); return aiDown; }

                var input = JsonSerializer.Serialize(new
                {
                    topic = message.Topic.ToString(),
                    reference = message.Reference,
                    message = message.Message
                });
                var answer = await ai.AskAsync(new AiJsonRequest(AiPrompts.ContactSorting, input, $"Contact message from {message.Name}"), ct);
                if (answer.IsFailure) return Failed(check, answer.Error, () => false);

                var spam = Bool(answer.Value.Json, "spam");
                check.SortedByAi(spam, Bool(answer.Value.Json, "urgent"), answer.Value.Text("summary"),
                                 answer.Value.Text("topic"), answer.Value.CallId);
                if (spam) message.MarkSpamByAi();
                return aiDown;
            }

            default:
                check.Replace();
                return aiDown;
        }
    }

    /// <summary>Asks a verdict job and applies the answer.</summary>
    private async Task<string?> AskVerdictAsync(ModerationCheck check, string promptKey, string input, string subject,
                                                string? aiDown, Func<ModerationVerdict, bool> needsPerson,
                                                Action<bool> apply, Func<bool> whenNotChecked, CancellationToken ct,
                                                string labelField = "rule")
    {
        if (aiDown is not null)
        {
            check.NotCheckedByAi(aiDown, whenNotChecked());
            return aiDown;
        }

        var answer = await ai.AskAsync(new AiJsonRequest(promptKey, input, subject), ct);
        if (answer.IsFailure) return Failed(check, answer.Error, whenNotChecked);

        var verdict = Verdict(answer.Value.Text("verdict"));
        var toPerson = needsPerson(verdict);
        check.AnsweredByAi(verdict, answer.Value.Text("reason"), answer.Value.Text(labelField),
                           answer.Value.CallId, toPerson);
        apply(toPerson);
        return null;
    }

    /// <summary>The AI didn't answer: off / cap → don't ask again this run; service down / bad answer → try again
    /// next run, up to 3 times.</summary>
    private static string? Failed(ModerationCheck check, Error error, Func<bool> whenNotChecked)
    {
        if (error == AiErrors.Disabled || error == AiErrors.SpendCapReached)
        {
            var why = error == AiErrors.Disabled ? "Not checked: AI is off." : "Not checked: this month's AI limit was reached.";
            check.NotCheckedByAi(why, whenNotChecked());
            return why;
        }

        if (check.AiFailed(error.Message))
            check.NotCheckedByAi($"Not checked: the AI failed {ModerationCheck.MaxAiAttempts} times.", whenNotChecked());
        return null;
    }

    // ───────────────────────────── Things from before checks began ─────────────────────────────

    /// <summary>Queues a few dishes and shops that have never been checked (they stay visible meanwhile).</summary>
    private async Task QueueOldContentAsync(CancellationToken ct)
    {
        var oldDishes = await checks.GetUncheckedDishesAsync(BackfillPerRun, ct);
        foreach (var (id, text, country) in oldDishes)
            checks.Add(ModerationCheck.Queue(ModerationKind.Dish, id, text, country));

        var oldShops = await checks.GetUncheckedShopsAsync(BackfillPerRun, ct);
        foreach (var (id, name, country) in oldShops)
            checks.Add(ModerationCheck.Queue(ModerationKind.ShopName, id, name, country));

        if (oldDishes.Count + oldShops.Count > 0) await unitOfWork.SaveChangesAsync(ct);
    }

    // ───────────────────────────── Reading answers ─────────────────────────────

    private static ModerationVerdict Verdict(string? text) => text?.Trim().ToLowerInvariant() switch
    {
        "ok" or "approve" => ModerationVerdict.Ok,
        "block" or "reject" => ModerationVerdict.NotAllowed,
        _ => ModerationVerdict.NeedsLook   // "review", or anything unexpected: a person looks
    };

    private static bool Bool(JsonElement json, string name) =>
        json.TryGetProperty(name, out var v) && (v.ValueKind == JsonValueKind.True
                                                 || (v.ValueKind == JsonValueKind.String
                                                     && string.Equals(v.GetString(), "true", StringComparison.OrdinalIgnoreCase)));

    private static string CountryName(Country country) =>
        country == Country.UnitedKingdom ? "United Kingdom" : country.ToString();
}
