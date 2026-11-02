using Phloem.Domain.Enums;

namespace Phloem.Application.Features.Orders.Dtos;

/// <summary>One row of Admin → Orders.</summary>
/// <param name="OrderNumber">E.g. "PH-261016-482193" (opens the order).</param>
/// <param name="ShopId">The shop.</param>
/// <param name="ShopName">The shop's name when the order was placed.</param>
/// <param name="CustomerName">The customer's full name.</param>
/// <param name="Country">Where it's delivered.</param>
/// <param name="Status">Where the order is now.</param>
/// <param name="Total">What the customer pays (or paid).</param>
/// <param name="Currency">The order's currency.</param>
/// <param name="ItemCount">How many items.</param>
/// <param name="PlacedAt">When it was placed (UTC).</param>
/// <param name="CourierName">Who is delivering it; null until it's sent out.</param>
/// <param name="NeedsRefund">Paid, then cancelled: money to give back.</param>
public sealed record AdminOrderSummaryDto(
    string OrderNumber,
    Guid ShopId,
    string ShopName,
    string CustomerName,
    Country Country,
    OrderStatus Status,
    decimal Total,
    Currency Currency,
    int ItemCount,
    DateTime PlacedAt,
    string? CourierName,
    bool NeedsRefund);
