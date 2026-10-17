using Phloem.Domain.Entities;
using Phloem.Domain.Enums;

namespace Phloem.Application.Features.Orders.Dtos;

/// <summary>One order created by "Place order" (one per shop in the basket).</summary>
/// <param name="OrderNumber">Shown to the customer, e.g. "PH-261016-482193".</param>
/// <param name="ShopName">The shop this order is from.</param>
/// <param name="Total">Order total at placing time.</param>
/// <param name="Currency">The order's currency.</param>
public sealed record PlacedOrderDto(string OrderNumber, string ShopName, decimal Total, Currency Currency)
{
    /// <summary>Maps a newly placed order.</summary>
    /// <param name="order">The order.</param>
    public static PlacedOrderDto From(Order order) =>
        new(order.OrderNumber, order.VendorName, order.Total, order.Currency);
}