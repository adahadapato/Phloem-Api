using Phloem.Domain.Entities;
using Phloem.Domain.Enums;

namespace Phloem.Application.Features.Shipments.Dtos;

/// <summary>
/// The "Booked!" screen: the tracking number, the estimated price, and where to drop the parcel off.
/// The tracking number is new to the app, so the booking command returns it.
/// </summary>
/// <param name="TrackingNumber">e.g. "PHS-261017-482193".</param>
/// <param name="EstimatedPrice">Price from the estimated weight.</param>
/// <param name="Currency">Origin currency.</param>
/// <param name="DropOffAddress">Where to drop it off.</param>
/// <param name="DropOffHours">The hub's opening hours.</param>
public sealed record BookedShipmentDto(
    string TrackingNumber,
    decimal EstimatedPrice,
    Currency Currency,
    string DropOffAddress,
    string DropOffHours)
{
    /// <summary>Maps the new shipment and its route.</summary>
    /// <param name="shipment">The booked shipment.</param>
    /// <param name="route">Its route.</param>
    /// <returns>The DTO.</returns>
    public static BookedShipmentDto From(Shipment shipment, ShipmentRoute route) => new(
        shipment.TrackingNumber,
        shipment.EstimatedPrice,
        shipment.Currency,
        route.DropOffAddress,
        route.DropOffHours);
}
