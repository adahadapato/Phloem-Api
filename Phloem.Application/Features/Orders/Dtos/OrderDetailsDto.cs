using Phloem.Application.Features.Deliveries.Dtos;
using Phloem.Domain.Enums;

namespace Phloem.Application.Features.Orders.Dtos;

/// <summary>
/// Order details page. Used by the customer AND by the shop (for paid orders);
/// <see cref="ViewedBySeller"/> tells the app which buttons to show.
/// </summary>
/// <param name="OrderNumber">The order number.</param>
/// <param name="ShopName">The shop.</param>
/// <param name="CustomerName">The customer's name (useful to the seller).</param>
/// <param name="Status">Where the order is.</param>
/// <param name="Lines">The dishes (live prices and notes while unpaid).</param>
/// <param name="Subtotal">Sum of the lines (available ones only while unpaid).</param>
/// <param name="DeliveryFee">Unpaid: today's fee. Otherwise: the fee paid.</param>
/// <param name="Total">Subtotal + DeliveryFee.</param>
/// <param name="Currency">The order's currency.</param>
/// <param name="DeliveryWindow">ASAP / this evening / tomorrow.</param>
/// <param name="DeliveryLabel">Address label, e.g. "Home".</param>
/// <param name="DeliveryAddress">Full address.</param>
/// <param name="PlacedAt">When placed (UTC).</param>
/// <param name="PaidAt">When paid (UTC); null if unpaid.</param>
/// <param name="DeliveredAt">When delivered (UTC); null until then.</param>
/// <param name="CancelledAt">When cancelled (UTC); null if not cancelled.</param>
/// <param name="CanCancel">Customer only: whether Cancel should show.</param>
/// <param name="CancelSecondsLeft">Paid orders: seconds left to cancel (countdown); null = no deadline.</param>
/// <param name="NeedsRefund">A paid order that was cancelled.</param>
/// <param name="ViewedBySeller">True when the shop is viewing (shows Accept / Send out / Delivered).</param>
/// <param name="DeclinedByShop">The shop couldn't make it (cancelled and refunded).</param>
/// <param name="DeclineReason">The shop's reason, when <paramref name="DeclinedByShop"/>.</param>
/// <param name="DeliveryRecipientName">Who receives it (the address's name, else the customer's).</param>
/// <param name="DeliveryPhone">Who the seller calls (the address's phone, else the customer's); null if neither.</param>
/// <param name="CourierName">Who is delivering it (or delivered it); null until it's sent out.</param>
/// <param name="DeliveryProof">The photo and handover details; null until delivered.</param>
/// <param name="CanMarkDelivered">The viewer is this order's courier and it's out for delivery (the seller delivering
/// it themselves): show "Delivered" (take a photo).</param>
public sealed record OrderDetailsDto(
    string OrderNumber,
    string ShopName,
    string CustomerName,
    OrderStatus Status,
    IReadOnlyList<OrderLineDto> Lines,
    decimal Subtotal,
    decimal DeliveryFee,
    decimal Total,
    Currency Currency,
    DeliveryWindow DeliveryWindow,
    string DeliveryLabel,
    string DeliveryAddress,
    DateTime PlacedAt,
    DateTime? PaidAt,
    DateTime? DeliveredAt,
    DateTime? CancelledAt,
    bool CanCancel,
    int? CancelSecondsLeft,
    bool NeedsRefund,
    bool ViewedBySeller,
    bool DeclinedByShop,
    string? DeclineReason,
    string DeliveryRecipientName,
    string? DeliveryPhone,
    string? CourierName,
    DeliveryProofDto? DeliveryProof,
    bool CanMarkDelivered);