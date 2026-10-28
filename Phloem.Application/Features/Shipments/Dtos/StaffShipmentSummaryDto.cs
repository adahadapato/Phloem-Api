using Phloem.Domain.Entities;
using Phloem.Domain.Enums;

namespace Phloem.Application.Features.Shipments.Dtos;

/// <summary>
/// One row in the staff Shipments list:
/// "PHS-261017-482193 · Ghana → UK · Ama Mensah → Kofi Boateng · Express · 7.4 kg · Received at hub".
/// Names, never Ids.
/// </summary>
/// <param name="TrackingNumber">The tracking number (the handle for staff actions).</param>
/// <param name="FromCountry">Origin.</param>
/// <param name="ToCountry">Destination.</param>
/// <param name="SenderName">Sender.</param>
/// <param name="RecipientName">Recipient.</param>
/// <param name="Status">Where it is.</param>
/// <param name="Speed">The chosen speed.</param>
/// <param name="ItemCount">Total number of items.</param>
/// <param name="EstimatedWeightKg">Weight entered at booking.</param>
/// <param name="WeighedWeightKg">Weight measured at the hub; null until weighed.</param>
/// <param name="Price">Final price once weighed, otherwise the estimate.</param>
/// <param name="Currency">Origin currency.</param>
/// <param name="BookedAt">When booked (UTC).</param>
/// <param name="CourierName">Who is delivering it (or delivered it); null until it's out for delivery.</param>
public sealed record StaffShipmentSummaryDto(
    string TrackingNumber,
    Country FromCountry,
    Country ToCountry,
    string SenderName,
    string RecipientName,
    ShipmentStatus Status,
    DeliverySpeed Speed,
    int ItemCount,
    decimal EstimatedWeightKg,
    decimal? WeighedWeightKg,
    decimal Price,
    Currency Currency,
    DateTime BookedAt,
    string? CourierName)
{
    /// <summary>Maps a shipment (with items).</summary>
    /// <param name="s">The shipment.</param>
    /// <returns>The DTO.</returns>
    public static StaffShipmentSummaryDto From(Shipment s) => new(
        s.TrackingNumber,
        s.FromCountry,
        s.ToCountry,
        s.SenderName,
        s.RecipientName,
        s.Status,
        s.Speed,
        s.Items.Sum(i => i.Quantity),
        s.EstimatedWeightKg,
        s.WeighedWeightKg,
        s.FinalPrice ?? s.EstimatedPrice,
        s.Currency,
        s.CreatedAt,
        s.CourierName);
}
