using Phloem.Domain.Entities;
using Phloem.Domain.Enums;

namespace Phloem.Application.Features.Shipments.Dtos;

/// <summary>One card in "My shipments": "Ghana → UK · to Kofi Boateng · 3 items · GHS 417.50 · Received at hub".</summary>
/// <param name="TrackingNumber">The tracking number (opens the details).</param>
/// <param name="FromCountry">Origin.</param>
/// <param name="ToCountry">Destination.</param>
/// <param name="RecipientName">Recipient.</param>
/// <param name="Status">Where it is.</param>
/// <param name="Speed">The chosen speed.</param>
/// <param name="ItemCount">Total number of items.</param>
/// <param name="Price">Final price once weighed, otherwise the estimate.</param>
/// <param name="IsPriceFinal">True once weighed (the app shows "Estimated" otherwise).</param>
/// <param name="Currency">Origin currency.</param>
/// <param name="BookedAt">When booked (UTC).</param>
/// <param name="CanPay">Weighed and waiting for payment ("Pay now").</param>
/// <param name="CanCancel">Whether Cancel should show.</param>
public sealed record ShipmentSummaryDto(
    string TrackingNumber,
    Country FromCountry,
    Country ToCountry,
    string RecipientName,
    ShipmentStatus Status,
    DeliverySpeed Speed,
    int ItemCount,
    decimal Price,
    bool IsPriceFinal,
    Currency Currency,
    DateTime BookedAt,
    bool CanPay,
    bool CanCancel)
{
    /// <summary>Maps a shipment (with items).</summary>
    /// <param name="s">The shipment.</param>
    /// <returns>The DTO.</returns>
    public static ShipmentSummaryDto From(Shipment s) => new(
        s.TrackingNumber,
        s.FromCountry,
        s.ToCountry,
        s.RecipientName,
        s.Status,
        s.Speed,
        s.Items.Sum(i => i.Quantity),
        s.FinalPrice ?? s.EstimatedPrice,
        IsPriceFinal: s.FinalPrice is not null,
        s.Currency,
        s.CreatedAt,
        CanPay: s.Status == ShipmentStatus.ReceivedAtHub,
        s.CanCancel);
}
