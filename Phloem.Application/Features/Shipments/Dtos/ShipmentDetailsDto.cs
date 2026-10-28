using Phloem.Application.Features.Deliveries.Dtos;
using Phloem.Domain.Entities;
using Phloem.Domain.Enums;

namespace Phloem.Application.Features.Shipments.Dtos;

/// <summary>The FULL shipment page, for the sender and for staff. Never used for public tracking.</summary>
/// <param name="TrackingNumber">The tracking number.</param>
/// <param name="FromCountry">Origin.</param>
/// <param name="ToCountry">Destination.</param>
/// <param name="Status">Where it is.</param>
/// <param name="Speed">The chosen speed.</param>
/// <param name="SenderName">Sender's name.</param>
/// <param name="SenderPhone">Sender's contact phone.</param>
/// <param name="RecipientName">Recipient's name.</param>
/// <param name="RecipientPhone">Recipient's phone.</param>
/// <param name="RecipientAddressLine">Street.</param>
/// <param name="RecipientCity">City / town.</param>
/// <param name="RecipientRegionOrPostcode">Region / postcode.</param>
/// <param name="Items">Declared items.</param>
/// <param name="TotalDeclaredValue">Sum of declared values.</param>
/// <param name="EstimatedWeightKg">Weight entered at booking.</param>
/// <param name="WeighedWeightKg">Weight measured at the hub; null until weighed.</param>
/// <param name="EstimatedPrice">Price at booking.</param>
/// <param name="FinalPrice">Price after weighing; null until weighed.</param>
/// <param name="Currency">Origin currency.</param>
/// <param name="DropOffAddress">Where to drop it off; null if the route no longer exists.</param>
/// <param name="DropOffHours">The hub's opening hours; null if the route no longer exists.</param>
/// <param name="EstimatedArrivalFrom">Earliest expected arrival; null until it's in transit.</param>
/// <param name="EstimatedArrivalTo">Latest expected arrival; null until it's in transit.</param>
/// <param name="DeliveredAt">When delivered (UTC); null until then.</param>
/// <param name="Timeline">Tracking steps, oldest first, with notes.</param>
/// <param name="CanCancel">Whether Cancel should show.</param>
/// <param name="CanPay">Weighed and waiting for payment ("Pay now").</param>
/// <param name="NeedsRefund">Paid, then cancelled.</param>
/// <param name="CourierName">Who is delivering it (or delivered it); null until it's out for delivery.</param>
/// <param name="DeliveryProof">The photo and handover details; null until delivered.</param>
public sealed record ShipmentDetailsDto(
    string TrackingNumber,
    Country FromCountry,
    Country ToCountry,
    ShipmentStatus Status,
    DeliverySpeed Speed,
    string SenderName,
    string SenderPhone,
    string RecipientName,
    string RecipientPhone,
    string RecipientAddressLine,
    string RecipientCity,
    string RecipientRegionOrPostcode,
    IReadOnlyList<ShipmentItemDto> Items,
    decimal TotalDeclaredValue,
    decimal EstimatedWeightKg,
    decimal? WeighedWeightKg,
    decimal EstimatedPrice,
    decimal? FinalPrice,
    Currency Currency,
    string? DropOffAddress,
    string? DropOffHours,
    DateOnly? EstimatedArrivalFrom,
    DateOnly? EstimatedArrivalTo,
    DateTime? DeliveredAt,
    IReadOnlyList<ShipmentEventDto> Timeline,
    bool CanCancel,
    bool CanPay,
    bool NeedsRefund,
    string? CourierName,
    DeliveryProofDto? DeliveryProof)
{
    /// <summary>Maps a shipment (with items and timeline).</summary>
    /// <param name="s">The shipment.</param>
    /// <param name="route">Its route, for the drop-off hub; null if the route no longer exists.</param>
    /// <param name="proof">The proof of delivery with a fresh photo link (DeliveryPhotos.ToDtoAsync); null if none.</param>
    /// <returns>The DTO.</returns>
    public static ShipmentDetailsDto From(Shipment s, ShipmentRoute? route, DeliveryProofDto? proof) => new(
        s.TrackingNumber,
        s.FromCountry,
        s.ToCountry,
        s.Status,
        s.Speed,
        s.SenderName,
        s.SenderPhone.Value,
        s.RecipientName,
        s.RecipientPhone.Value,
        s.RecipientAddressLine,
        s.RecipientCity,
        s.RecipientRegionOrPostcode,
        s.Items.Select(ShipmentItemDto.From).ToList(),
        s.TotalDeclaredValue,
        s.EstimatedWeightKg,
        s.WeighedWeightKg,
        s.EstimatedPrice,
        s.FinalPrice,
        s.Currency,
        route?.DropOffAddress,
        route?.DropOffHours,
        s.EstimatedArrival?.From,
        s.EstimatedArrival?.To,
        s.DeliveredAtUtc,
        s.Timeline.Select(ShipmentEventDto.WithNote).ToList(),
        s.CanCancel,
        CanPay: s.Status == ShipmentStatus.ReceivedAtHub,
        s.NeedsRefund,
        s.CourierName,
        proof);
}
