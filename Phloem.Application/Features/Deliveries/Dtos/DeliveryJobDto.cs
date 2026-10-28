using Phloem.Domain.Enums;

namespace Phloem.Application.Features.Deliveries.Dtos;

/// <summary>
/// One card in the courier's "My deliveries": where to collect it, who gets it, where, and what's in it.
/// </summary>
/// <param name="Kind">A food order or a parcel.</param>
/// <param name="Reference">The order number or tracking number (the handle for "Delivered").</param>
/// <param name="CollectFrom">Food: the shop's name. Parcels: "Phloem hub".</param>
/// <param name="CollectPhone">The shop's phone (food), if it has one.</param>
/// <param name="RecipientName">Who it's for.</param>
/// <param name="RecipientPhone">Who to call on arrival; null if unknown.</param>
/// <param name="Address">Where to take it.</param>
/// <param name="Country">The delivery country.</param>
/// <param name="ItemsSummary">e.g. "Jollof Rice ×2, Kelewele ×1" or "3 items, 7.4 kg".</param>
/// <param name="DeliveryWindow">Food: when the customer wants it; null for parcels.</param>
/// <param name="AssignedAt">When it was given to this courier (UTC).</param>
/// <param name="DeliveredAt">When it was delivered (UTC); null while still to deliver.</param>
/// <param name="HandoverSummary">Delivered ones: e.g. "Left at the front door".</param>
public sealed record DeliveryJobDto(
    DeliveryKind Kind,
    string Reference,
    string CollectFrom,
    string? CollectPhone,
    string RecipientName,
    string? RecipientPhone,
    string Address,
    Country Country,
    string ItemsSummary,
    DeliveryWindow? DeliveryWindow,
    DateTime? AssignedAt,
    DateTime? DeliveredAt,
    string? HandoverSummary);
