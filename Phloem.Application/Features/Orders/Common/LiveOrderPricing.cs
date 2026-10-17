using Phloem.Application.Features.Orders.Dtos;
using Phloem.Domain.Entities;
using Phloem.Domain.Enums;
using Phloem.Domain.Interfaces;

namespace Phloem.Application.Features.Orders.Common;

/// <summary>
/// Works out how an order looks RIGHT NOW. Unpaid orders use today's prices and mark dishes
/// that can't be ordered as out of stock; any other order shows exactly what was paid.
/// </summary>
/// <remarks>
/// Pure calculation: callers load the dishes / pricing first (one query per list, not per order).
/// The same rules are applied for real by <see cref="Order.PrepareForPayment"/> when the customer pays.
/// </remarks>
internal static class LiveOrderPricing
{
    /// <summary>True if the order should be shown with live prices.</summary>
    /// <param name="order">The order.</param>
    public static bool IsLive(Order order) => order.Status == OrderStatus.AwaitingPayment;

    /// <summary>Builds the order's lines for display.</summary>
    /// <param name="order">The order.</param>
    /// <param name="currentDishes">The order's dishes as they are now (missing = deleted).</param>
    /// <param name="storage">Turns stored image paths into URLs.</param>
    public static IReadOnlyList<OrderLineDto> Lines(Order order, IReadOnlyDictionary<Guid, Dish> currentDishes,
                                                    IFileStorage storage)
    {
        return order.Lines.Select(line =>
        {
            var imageUrl = line.DishImagePath is null ? null : storage.GetPublicUrl(line.DishImagePath);

            if (!IsLive(order))
                return new OrderLineDto(line.DishId, line.DishName, imageUrl, line.Quantity, line.UnitPrice,
                                        PriceWhenAdded: null, PriceChanged: false, IsOutOfStock: false, line.LineTotal);

            if (!currentDishes.TryGetValue(line.DishId, out var dish) || !dish.CanBeOrdered)
                return new OrderLineDto(line.DishId, line.DishName, imageUrl, line.Quantity, line.UnitPrice,
                                        line.UnitPrice, PriceChanged: false, IsOutOfStock: true, LineTotal: 0);

            return new OrderLineDto(line.DishId, dish.Name, imageUrl, line.Quantity, dish.Price,
                                    line.UnitPrice, dish.Price != line.UnitPrice, IsOutOfStock: false,
                                    dish.Price * line.Quantity);
        }).ToList();
    }

    /// <summary>
    /// The delivery fee to show: today's fee for an unpaid order (if it can be worked out),
    /// otherwise the fee stored on the order.
    /// </summary>
    /// <param name="order">The order.</param>
    /// <param name="vendor">The shop as it is now; null if it no longer exists.</param>
    /// <param name="pricing">The shop's country's delivery pricing; null if not set.</param>
    public static decimal DeliveryFee(Order order, Vendor? vendor, DeliveryPricing? pricing)
    {
        if (!IsLive(order) || vendor is null || pricing is null) return order.DeliveryFee;

        var fee = pricing.FeeFor(vendor, order.DeliveryWindow);
        return fee.IsSuccess ? fee.Value : order.DeliveryFee;
    }

    /// <summary>"Jollof Rice ×2, Kelewele ×1".</summary>
    /// <param name="order">The order.</param>
    public static string ItemsSummary(Order order) =>
        string.Join(", ", order.Lines.Select(l => $"{l.DishName} ×{l.Quantity}"));
}