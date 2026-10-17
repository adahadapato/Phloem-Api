using Phloem.Domain.Enums;

namespace Phloem.Application.Features.Shipments.Dtos;

/// <summary>"Send a parcel" step 4: one item as the app sends it.</summary>
/// <param name="Category">What kind of item (for customs).</param>
/// <param name="Description">e.g. "Gari, 2 kg bags".</param>
/// <param name="Quantity">How many.</param>
/// <param name="UnitDeclaredValue">Value of ONE item in the origin currency (for customs).</param>
public sealed record ShipmentItemInputDto(
    ShipmentItemCategory Category,
    string Description,
    int Quantity,
    decimal UnitDeclaredValue);
