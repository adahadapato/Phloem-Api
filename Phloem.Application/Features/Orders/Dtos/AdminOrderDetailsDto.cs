using Phloem.Application.Features.Deliveries.Dtos;
using Phloem.Domain.Enums;

namespace Phloem.Application.Features.Orders.Dtos;

/// <summary>Admin → Orders → one order: everything staff need to answer a question about it (read-only).</summary>
/// <param name="OrderNumber">E.g. "PH-261016-482193".</param>
/// <param name="Status">Where the order is now.</param>
/// <param name="Country">Where it's delivered.</param>
/// <param name="ShopId">The shop.</param>
/// <param name="ShopName">The shop's name when the order was placed.</param>
/// <param name="CustomerId">The customer's account.</param>
/// <param name="CustomerName">The customer's full name.</param>
/// <param name="CustomerEmail">The customer's sign-in email; null if the account is gone.</param>
/// <param name="CustomerPhone">The customer's own phone; null if none.</param>
/// <param name="Lines">The dishes, at the prices frozen when the order was placed (live prices while unpaid).</param>
/// <param name="Subtotal">Dishes total.</param>
/// <param name="DeliveryFee">Delivery fee.</param>
/// <param name="Total">Subtotal + delivery.</param>
/// <param name="Currency">The order's currency.</param>
/// <param name="CommissionAmount">Phloem's commission (set on delivery).</param>
/// <param name="SellerEarnings">What the shop earned (set on delivery).</param>
/// <param name="DeliveryWindow">The delivery window chosen.</param>
/// <param name="DeliveryLabel">The address's label, e.g. "Home".</param>
/// <param name="DeliveryAddress">The address, as one line.</param>
/// <param name="DeliveryRecipientName">Who receives it.</param>
/// <param name="DeliveryPhone">The phone on the address; null if none.</param>
/// <param name="PlacedAt">When it was placed (UTC).</param>
/// <param name="PaidAt">When it was paid.</param>
/// <param name="PaymentReference">The payment provider's reference.</param>
/// <param name="DeliveredAt">When it was delivered.</param>
/// <param name="CancelledAt">When it was cancelled.</param>
/// <param name="NeedsRefund">Paid, then cancelled.</param>
/// <param name="DeclinedByShop">The shop couldn't make it.</param>
/// <param name="DeclineReason">The shop's reason.</param>
/// <param name="CourierId">The courier's account; null until it's sent out.</param>
/// <param name="CourierName">The courier's name.</param>
/// <param name="DeliveryProof">The photo and handover details; null until delivered.</param>
/// <param name="Timeline">Every step, oldest first, with full names.</param>
public sealed record AdminOrderDetailsDto(
    string OrderNumber,
    OrderStatus Status,
    Country Country,
    Guid ShopId,
    string ShopName,
    Guid CustomerId,
    string CustomerName,
    string? CustomerEmail,
    string? CustomerPhone,
    IReadOnlyList<OrderLineDto> Lines,
    decimal Subtotal,
    decimal DeliveryFee,
    decimal Total,
    Currency Currency,
    decimal? CommissionAmount,
    decimal? SellerEarnings,
    DeliveryWindow DeliveryWindow,
    string DeliveryLabel,
    string DeliveryAddress,
    string DeliveryRecipientName,
    string? DeliveryPhone,
    DateTime PlacedAt,
    DateTime? PaidAt,
    string? PaymentReference,
    DateTime? DeliveredAt,
    DateTime? CancelledAt,
    bool NeedsRefund,
    bool DeclinedByShop,
    string? DeclineReason,
    Guid? CourierId,
    string? CourierName,
    DeliveryProofDto? DeliveryProof,
    IReadOnlyList<OrderEventDto> Timeline);
