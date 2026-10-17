using Phloem.Domain.Entities;
using Phloem.Domain.Enums;

namespace Phloem.Application.Features.Shipments.Dtos;

/// <summary>"Send a parcel" step 1: one route the customer can book, with its hub and speeds.</summary>
/// <param name="FromCountry">Origin.</param>
/// <param name="ToCountry">Destination.</param>
/// <param name="Currency">Origin currency (all prices).</param>
/// <param name="DropOffAddress">Where to drop the parcel off.</param>
/// <param name="DropOffHours">The hub's opening hours.</param>
/// <param name="Speeds">Offered speeds, in speed order.</param>
public sealed record ShipmentRouteOptionDto(
    Country FromCountry,
    Country ToCountry,
    Currency Currency,
    string DropOffAddress,
    string DropOffHours,
    IReadOnlyList<ShipmentSpeedOptionDto> Speeds)
{
    /// <summary>Maps a route (with rates).</summary>
    /// <param name="route">The route.</param>
    /// <returns>The DTO.</returns>
    public static ShipmentRouteOptionDto From(ShipmentRoute route) => new(
        route.FromCountry,
        route.ToCountry,
        route.Currency,
        route.DropOffAddress,
        route.DropOffHours,
        route.Rates.OrderBy(r => r.Speed).Select(ShipmentSpeedOptionDto.From).ToList());
}
