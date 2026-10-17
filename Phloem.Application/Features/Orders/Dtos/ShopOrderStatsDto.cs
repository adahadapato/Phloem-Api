namespace Phloem.Application.Features.Orders.Dtos;

/// <summary>Seller dashboard tiles and weekly chart.</summary>
/// <param name="NewOrders">Paid and waiting to be accepted.</param>
/// <param name="Preparing">Accepted, being cooked.</param>
/// <param name="OutForDelivery">On the way.</param>
/// <param name="Delivered">Delivered (all time).</param>
/// <param name="TotalOrders">Every paid order (all time, any status except unpaid).</param>
/// <param name="WeeklySales">The last 7 days, oldest first, including days with 0 orders.</param>
public sealed record ShopOrderStatsDto(
    int NewOrders,
    int Preparing,
    int OutForDelivery,
    int Delivered,
    int TotalOrders,
    IReadOnlyList<DailySalesDto> WeeklySales);