using Phloem.Domain.Enums;

namespace Phloem.Application.Features.Orders.Dtos;

/// <summary>
/// One card in the Orders tab: "PH-261016-482193 · Mama Ama's Kitchen · Jollof Rice ×2, Kelewele ×1 · GHS 98.00".
/// </summary>
/// <param name="OrderNumber">The order number.</param>
/// <param name="ShopName">The shop.</param>
/// <param name="Status">Where the order is.</param>
/// <param name="PlacedAt">When it was placed (UTC).</param>
/// <param name="PaidAt">When it was paid (UTC); null if unpaid.</param>
/// <param name="ItemCount">Total number of items.</param>
/// <param name="ItemsSummary">Short text of the dishes, e.g. "Jollof Rice ×2, Kelewele ×1".</param>
/// <param name="ImageUrl">First dish's thumbnail, or null.</param>
/// <param name="Total">Unpaid: today's total (available dishes + today's fee). Otherwise: total paid.</param>
/// <param name="Currency">The order's currency.</param>
/// <param name="HasPriceChanges">Unpaid only: some prices changed since they were added.</param>
/// <param name="OutOfStockCount">Unpaid only: how many dishes are out of stock.</param>
/// <param name="CanCancel">Whether the Cancel button should show.</param>
public sealed record OrderSummaryDto(
    string OrderNumber,
    string ShopName,
    OrderStatus Status,
    DateTime PlacedAt,
    DateTime? PaidAt,
    int ItemCount,
    string ItemsSummary,
    string? ImageUrl,
    decimal Total,
    Currency Currency,
    bool HasPriceChanges,
    int OutOfStockCount,
    bool CanCancel);