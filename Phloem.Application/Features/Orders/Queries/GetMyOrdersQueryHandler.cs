using MediatR;
using Phloem.Application.Features.Orders.Common;
using Phloem.Application.Features.Orders.Dtos;
using Phloem.Domain.Entities;
using Phloem.Domain.Enums;
using Phloem.Domain.Interfaces;
using Phloem.Domain.Shared.Results;

namespace Phloem.Application.Features.Orders.Queries;

/// <summary>Handles <see cref="GetMyOrdersQuery"/>.</summary>
public sealed class GetMyOrdersQueryHandler(
    ICurrentUser currentUser,
    IOrderRepository orders,
    IDishRepository dishes,
    IDeliveryPricingRepository pricings,
    IFileStorage storage,
    TimeProvider time)
    : IRequestHandler<GetMyOrdersQuery, Result<PagedResult<OrderSummaryDto>>>
{
    /// <summary>
    /// Loads one page of orders, then (in ONE query) the current dishes of the unpaid ones,
    /// and builds each card with live prices where needed.
    /// </summary>
    /// <exception cref="InvalidOperationException">Called without a signed-in user (endpoint misconfigured).</exception>
    public async Task<Result<PagedResult<OrderSummaryDto>>> Handle(GetMyOrdersQuery query, CancellationToken ct)
    {
        var userId = currentUser.UserId
            ?? throw new InvalidOperationException("This action requires a signed-in user.");

        var page = new PageRequest { Page = query.Page, PageSize = query.PageSize };
        var paged = await orders.GetForCustomerAsync(userId, page, ct);

        // Current dishes for every unpaid order on this page, in one query
        var liveDishIds = paged.Items
            .Where(LiveOrderPricing.IsLive)
            .SelectMany(o => o.Lines.Select(l => l.DishId))
            .Distinct()
            .ToList();

        var current = liveDishIds.Count == 0
            ? new Dictionary<Guid, Dish>()
            : (await dishes.GetByIdsAsync(liveDishIds, ct)).ToDictionary(d => d.Id);

        // Delivery pricing per country involved (at most a handful)
        var pricingByCountry = new Dictionary<Country, DeliveryPricing?>();
        foreach (var country in current.Values.Select(d => d.Vendor.Country).Distinct())
            pricingByCountry[country] = await pricings.GetByCountryAsync(country, ct);

        var now = time.GetUtcNow().UtcDateTime;

        var items = paged.Items.Select(order =>
        {
            var lines = LiveOrderPricing.Lines(order, current, storage);

            var vendor = order.Lines
                .Select(l => current.GetValueOrDefault(l.DishId)?.Vendor)
                .FirstOrDefault(v => v is not null);
            var pricing = vendor is null ? null : pricingByCountry.GetValueOrDefault(vendor.Country);

            var subtotal = lines.Sum(l => l.LineTotal);
            var fee = LiveOrderPricing.DeliveryFee(order, vendor, pricing);

            return new OrderSummaryDto(
                order.OrderNumber,
                order.VendorName,
                order.Status,
                order.PlacedAtUtc,
                order.PaidAtUtc,
                order.Lines.Sum(l => l.Quantity),
                LiveOrderPricing.ItemsSummary(order),
                lines.FirstOrDefault()?.ImageUrl,
                subtotal + fee,
                order.Currency,
                lines.Any(l => l.PriceChanged),
                lines.Count(l => l.IsOutOfStock),
                order.CanCancel(now));
        }).ToList();

        return new PagedResult<OrderSummaryDto>(items, paged.TotalCount, paged.Page, paged.PageSize);
    }
}