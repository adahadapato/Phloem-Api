using Phloem.Domain.Enums;

namespace Phloem.Application.Features.Orders.Dtos;

/// <summary>
/// One row in the seller's "Shop orders" list:
/// "PH-261016-482193 · Ama Mensah · Jollof Rice ×2 · ASAP · Home, 12 Ring Road · GHS 98.00".
/// </summary>
/// <param name="OrderNumber">The order number (also the handle for Accept / Send out / Delivered).</param>
/// <param name="CustomerName">Who ordered.</param>
/// <param name="Status">Where the order is.</param>
/// <param name="PaidAt">When it was paid (UTC).</param>
/// <param name="ItemCount">Total number of items.</param>
/// <param name="ItemsSummary">e.g. "Jollof Rice ×2, Kelewele ×1".</param>
/// <param name="Total">Total paid.</param>
/// <param name="Currency">The order's currency.</param>
/// <param name="DeliveryWindow">ASAP / this evening / tomorrow.</param>
/// <param name="DeliveryLabel">Address label, e.g. "Home".</param>
/// <param name="DeliveryAddress">Full delivery address.</param>
/// <param name="DeliveryRecipientName">Who receives it (the address's name, else the customer's).</param>
/// <param name="DeliveryPhone">Who to call (the address's phone, else the customer's); null if neither.</param>
public sealed record ShopOrderSummaryDto(
    string OrderNumber,
    string CustomerName,
    OrderStatus Status,
    DateTime? PaidAt,
    int ItemCount,
    string ItemsSummary,
    decimal Total,
    Currency Currency,
    DeliveryWindow DeliveryWindow,
    string DeliveryLabel,
    string DeliveryAddress,
    string DeliveryRecipientName,
    string? DeliveryPhone);