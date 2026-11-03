using Phloem.Domain.Entities;
using Phloem.Domain.Enums;
using Phloem.Domain.Interfaces;

namespace Phloem.Application.Features.Moderation.Common;

/// <summary>
/// Adds a check of people's text for the background checker (it runs within about a minute). Called by the
/// handlers that save that text; the caller's save stores the check together with the change.
/// </summary>
/// <remarks>A newer check of the same dish or shop replaces any unfinished older one, so only the latest wording
/// is checked and decided.</remarks>
public sealed class ModerationQueue(IModerationCheckRepository checks)
{
    /// <summary>A dish's new or changed wording (the dish is already hidden: <see cref="ContentReviewStatus.Checking"/>).</summary>
    public async Task DishAsync(Dish dish, CancellationToken ct)
    {
        await ReplaceOpenAsync(ModerationKind.Dish, dish.Id, ct);
        checks.Add(ModerationCheck.Queue(ModerationKind.Dish, dish.Id, dish.ModerationText, dish.Country));
    }

    /// <summary>A shop's new name.</summary>
    public async Task ShopNameAsync(Vendor shop, CancellationToken ct)
    {
        await ReplaceOpenAsync(ModerationKind.ShopName, shop.Id, ct);
        checks.Add(ModerationCheck.Queue(ModerationKind.ShopName, shop.Id, shop.Name, shop.Country));
    }

    /// <summary>A dish name a seller suggested.</summary>
    public void DishName(DishCatalog suggestion) =>
        checks.Add(ModerationCheck.Queue(ModerationKind.DishNameSuggestion, suggestion.Id, suggestion.Name, null));

    /// <summary>A Contact us message.</summary>
    public void Contact(ContactMessage message) =>
        checks.Add(ModerationCheck.Queue(ModerationKind.ContactMessage, message.Id, message.Message, null));

    private async Task ReplaceOpenAsync(ModerationKind kind, Guid targetId, CancellationToken ct)
    {
        foreach (var open in await checks.GetOpenForTargetAsync(kind, targetId, ct)) open.Replace();
    }
}
