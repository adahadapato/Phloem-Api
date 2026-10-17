using Phloem.Domain.Entities;
using Phloem.Domain.Enums;

namespace Phloem.Application.Features.Shipments.Dtos;

/// <summary>One speed offered on a route, e.g. "Express · GHS 80.00 + GHS 45.00/kg · usually 3–5 days".</summary>
/// <param name="Speed">Standard / Express / Bulk.</param>
/// <param name="BaseCharge">Fixed amount per parcel.</param>
/// <param name="PricePerKg">Amount per chargeable kg (weight rounded up to the next 0.5 kg).</param>
/// <param name="MinTransitDays">Fewest days usually in transit.</param>
/// <param name="MaxTransitDays">Most days usually in transit.</param>
public sealed record ShipmentSpeedOptionDto(
    DeliverySpeed Speed,
    decimal BaseCharge,
    decimal PricePerKg,
    int MinTransitDays,
    int MaxTransitDays)
{
    /// <summary>Maps a rate.</summary>
    /// <param name="rate">The rate.</param>
    /// <returns>The DTO.</returns>
    public static ShipmentSpeedOptionDto From(ShipmentRate rate) =>
        new(rate.Speed, rate.BaseCharge, rate.PricePerKg, rate.MinTransitDays, rate.MaxTransitDays);
}
