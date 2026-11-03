using Phloem.Domain.Entities;
using Phloem.Domain.Enums;

namespace Phloem.Application.Features.Moderation.Dtos;

/// <summary>What the AI (or a person) made of some text, for the screens that show it (shop review page, Dish
/// names, Contact inbox).</summary>
/// <param name="State">Waiting for the AI, with a person, done…</param>
/// <param name="Verdict">The final verdict: the person's when decided, else the AI's (null when not checked).</param>
/// <param name="Reason">Why (or why it couldn't be checked).</param>
/// <param name="Label">Rule broken / close match / topic.</param>
/// <param name="DecidedByStaff">True when a person decided.</param>
public sealed record ModerationAdviceDto(
    ModerationState State,
    ModerationVerdict? Verdict,
    string? Reason,
    string? Label,
    bool DecidedByStaff)
{
    /// <summary>From a check.</summary>
    public static ModerationAdviceDto From(ModerationCheck c) => new(
        c.State,
        c.StaffVerdict ?? c.AiVerdict,
        c.State == ModerationState.DecidedByStaff ? c.StaffNote ?? c.AiReason : c.AiReason,
        c.AiLabel,
        c.State == ModerationState.DecidedByStaff);
}

/// <summary>One item in the Moderation queue (Admin → Moderation).</summary>
/// <param name="Id">The check.</param>
/// <param name="Kind">Dish or shop name.</param>
/// <param name="Title">The dish or shop name.</param>
/// <param name="ShopId">The shop (dish's shop, or the shop itself), for "Open the shop".</param>
/// <param name="ShopName">The shop's name.</param>
/// <param name="Country">The country.</param>
/// <param name="Text">The exact text checked.</param>
/// <param name="AiVerdict">What the AI said; null when it couldn't check.</param>
/// <param name="AiReason">The AI's reason, or why it couldn't check.</param>
/// <param name="AiRule">The rule the AI thinks it breaks.</param>
/// <param name="WhyHere">In plain words: why a person needs to look.</param>
/// <param name="IsLive">For dishes: false = hidden from buyers until decided. For shops: the shop is live.</param>
/// <param name="WaitingSince">When it was saved (UTC).</param>
public sealed record ModerationItemDto(
    Guid Id,
    ModerationKind Kind,
    string Title,
    Guid? ShopId,
    string? ShopName,
    Country? Country,
    string Text,
    ModerationVerdict? AiVerdict,
    string? AiReason,
    string? AiRule,
    string WhyHere,
    bool IsLive,
    DateTime WaitingSince);
