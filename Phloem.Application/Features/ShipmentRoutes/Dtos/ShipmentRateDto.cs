using Phloem.Domain.Entities;
using Phloem.Domain.Enums;

namespace Phloem.Application.Features.ShipmentRoutes.Dtos;

/// <summary>One speed's price and transit time on a route (admin view).</summary>
/// <param name="Speed">The speed.</param>
/// <param name="BaseCharge">Fixed amount per parcel.</param>
/// <param name="PricePerKg">Amount per chargeable kg.</param>
/// <param name="MinTransitDays">Fewest days in transit.</param>
/// <param name="MaxTransitDays">Most days in transit.</param>
public sealed record ShipmentRateDto(
    DeliverySpeed Speed,
    decimal BaseCharge,
    decimal PricePerKg,
    int MinTransitDays,
    int MaxTransitDays)
{
    /// <summary>Maps a rate.</summary>
    /// <param name="r">The rate.</param>
    /// <returns>The DTO.</returns>
    public static ShipmentRateDto From(ShipmentRate r) =>
        new(r.Speed, r.BaseCharge, r.PricePerKg, r.MinTransitDays, r.MaxTransitDays);
}
