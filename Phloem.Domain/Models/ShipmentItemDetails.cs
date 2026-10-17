using Phloem.Domain.Enums;

namespace Phloem.Domain.Models;

/// <summary>
/// Step 4 of the "Send a parcel" form: one item being shipped. An INPUT for
/// <see cref="Shipment.Book"/>; each becomes a <see cref="ShipmentItem"/>.
/// </summary>
/// <param name="Category">What kind of item (for customs).</param>
/// <param name="Description">e.g. "Gari, 2 kg bags".</param>
/// <param name="Quantity">How many.</param>
/// <param name="UnitDeclaredValue">Value of ONE item, in the route's currency (for customs).</param>
public sealed record ShipmentItemDetails(
    ShipmentItemCategory Category,
    string Description,
    int Quantity,
    decimal UnitDeclaredValue);