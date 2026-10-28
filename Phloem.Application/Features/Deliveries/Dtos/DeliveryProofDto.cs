using Phloem.Domain.Enums;

namespace Phloem.Application.Features.Deliveries.Dtos;

/// <summary>
/// "Proof of delivery" card on an order or parcel page: the photo and how it was handed over.
/// </summary>
/// <param name="PhotoUrl">A short-lived link to the photo (about 15 minutes); load the page again for a fresh one.</param>
/// <param name="Handover">How it was handed over.</param>
/// <param name="Summary">Ready to show, e.g. "Left at the front door" or "Handed to Ama".</param>
/// <param name="ReceivedBy">Who took it, when the courier said.</param>
/// <param name="Note">The courier's note, if any.</param>
/// <param name="CourierName">Who delivered it.</param>
/// <param name="DeliveredAt">When (UTC).</param>
public sealed record DeliveryProofDto(
    string PhotoUrl,
    HandoverMethod Handover,
    string Summary,
    string? ReceivedBy,
    string? Note,
    string? CourierName,
    DateTime DeliveredAt);
