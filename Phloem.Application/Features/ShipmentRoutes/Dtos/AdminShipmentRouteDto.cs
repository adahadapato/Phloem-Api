using Phloem.Domain.Entities;
using Phloem.Domain.Enums;

namespace Phloem.Application.Features.ShipmentRoutes.Dtos;

/// <summary>Admin → Shipping routes: one route, on or off, with its hub and rates.</summary>
/// <param name="FromCountry">Origin.</param>
/// <param name="ToCountry">Destination.</param>
/// <param name="Currency">Origin currency (all prices).</param>
/// <param name="DropOffAddress">Origin hub address.</param>
/// <param name="DropOffHours">Origin hub opening hours.</param>
/// <param name="IsActive">True = customers can book.</param>
/// <param name="Rates">Price and transit time per speed, in speed order.</param>
public sealed record AdminShipmentRouteDto(
    Country FromCountry,
    Country ToCountry,
    Currency Currency,
    string DropOffAddress,
    string DropOffHours,
    bool IsActive,
    IReadOnlyList<ShipmentRateDto> Rates)
{
    /// <summary>Maps a route (with rates).</summary>
    /// <param name="route">The route.</param>
    /// <returns>The DTO.</returns>
    public static AdminShipmentRouteDto From(ShipmentRoute route) => new(
        route.FromCountry,
        route.ToCountry,
        route.Currency,
        route.DropOffAddress,
        route.DropOffHours,
        route.IsActive,
        route.Rates.OrderBy(r => r.Speed).Select(ShipmentRateDto.From).ToList());
}
