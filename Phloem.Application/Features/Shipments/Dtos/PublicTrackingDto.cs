using Phloem.Domain.Entities;
using Phloem.Domain.Enums;

namespace Phloem.Application.Features.Shipments.Dtos;

/// <summary>
/// PUBLIC tracking (no sign-in), e.g. for the recipient. Deliberately limited: NO names, phones,
/// addresses, items, values, prices or timeline notes, so a guessed tracking number reveals nothing personal.
/// </summary>
/// <param name="TrackingNumber">The tracking number.</param>
/// <param name="FromCountry">Origin.</param>
/// <param name="ToCountry">Destination.</param>
/// <param name="Speed">The chosen speed.</param>
/// <param name="Status">Where it is.</param>
/// <param name="EstimatedArrivalFrom">Earliest expected arrival; null until it's in transit.</param>
/// <param name="EstimatedArrivalTo">Latest expected arrival; null until it's in transit.</param>
/// <param name="Timeline">Steps and times only (no notes), oldest first.</param>
public sealed record PublicTrackingDto(
    string TrackingNumber,
    Country FromCountry,
    Country ToCountry,
    DeliverySpeed Speed,
    ShipmentStatus Status,
    DateOnly? EstimatedArrivalFrom,
    DateOnly? EstimatedArrivalTo,
    IReadOnlyList<ShipmentEventDto> Timeline)
{
    /// <summary>Maps only the safe, public parts of a shipment.</summary>
    /// <param name="s">The shipment (with timeline).</param>
    /// <returns>The DTO.</returns>
    public static PublicTrackingDto From(Shipment s) => new(
        s.TrackingNumber,
        s.FromCountry,
        s.ToCountry,
        s.Speed,
        s.Status,
        s.EstimatedArrival?.From,
        s.EstimatedArrival?.To,
        s.Timeline.Select(ShipmentEventDto.WithoutNote).ToList());
}
