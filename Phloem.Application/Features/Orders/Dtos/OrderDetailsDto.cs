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
    bool ViewedBySeller);