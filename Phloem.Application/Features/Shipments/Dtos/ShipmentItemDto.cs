using Phloem.Domain.Entities;
using Phloem.Domain.Enums;

namespace Phloem.Application.Features.Shipments.Dtos;

/// <summary>One declared item on the shipment page (sender and staff views; never in public tracking).</summary>
/// <param name="Category">What kind of item (for customs).</param>
/// <param name="Description">The sender's description, e.g. "Gari, 2 kg bags".</param>
/// <param name="Quantity">How many.</param>
/// <param name="UnitDeclaredValue">Declared value of ONE item, in the shipment's (origin) currency.</param>
/// <param name="TotalDeclaredValue">Unit value × quantity.</param>
public sealed record ShipmentItemDto(
    ShipmentItemCategory Category,
    string Description,
    int Quantity,
    decimal UnitDeclaredValue,
    decimal TotalDeclaredValue)
{
    /// <summary>Maps a <see cref="ShipmentItem"/>.</summary>
    /// <param name="item">The item.</param>
    /// <returns>The DTO.</returns>
    public static ShipmentItemDto From(ShipmentItem item) =>
        new(item.Category, item.Description, item.Quantity, item.UnitDeclaredValue, item.TotalDeclaredValue);
}
