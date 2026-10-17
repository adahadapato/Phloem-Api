using MediatR;
using Phloem.Application.Features.Orders.Dtos;
using Phloem.Domain.Enums;
using Phloem.Domain.Interfaces;
using Phloem.Domain.Shared.Errors;
using Phloem.Domain.Shared.Results;

namespace Phloem.Application.Features.Orders.Queries;

/// <summary>Handles <see cref="GetShopOrderStatsQuery"/>.</summary>
public sealed class GetShopOrderStatsQueryHandler(
    ICurrentUser currentUser,
    IVendorRepository vendors,
    IOrderRepository orders,
    TimeProvider time)
    : IRequestHandler<GetShopOrderStatsQuery, Result<ShopOrderStatsDto>>
{
    /// <summary>How many days the sales chart shows.</summary>
    private const int ChartDays = 7;

    /// <summary>Counts are worked out live (never stored), so they can't drift out of date.</summary>
    /// <returns>The stats; or <see cref="VendorErrors.NotFound"/> if the user has no shop.</returns>
    /// <exception cref="InvalidOperationException">Called without a signed-in user (endpoint misconfigured).</exception>
    public async Task<Result<ShopOrderStatsDto>> Handle(GetShopOrderStatsQuery query, CancellationToken ct)
    {
        var userId = currentUser.UserId
            ?? throw new InvalidOperationException("This action requires a signed-in user.");

        var vendor = await vendors.GetByOwnerIdAsync(userId, ct);
        if (vendor is null) return VendorErrors.NotFound;

        var newOrders = await orders.CountForVendorAsync(vendor.Id, OrderStatus.Paid, ct);
        var preparing = await orders.CountForVendorAsync(vendor.Id, OrderStatus.Preparing, ct);
        var outForDelivery = await orders.CountForVendorAsync(vendor.Id, OrderStatus.OutForDelivery, ct);
        var delivered = await orders.CountForVendorAsync(vendor.Id, OrderStatus.Delivered, ct);
        var total = await orders.CountForVendorAsync(vendor.Id, status: null, ct);

        // Last 7 days, oldest first, with 0 for days without orders
        var today = DateOnly.FromDateTime(time.GetUtcNow().UtcDateTime);
        var from = today.AddDays(-(ChartDays - 1));
        var daily = await orders.GetDailyPaidCountsForVendorAsync(vendor.Id, from, today, ct);

        var week = Enumerable.Range(0, ChartDays)
            .Select(i => from.AddDays(i))
            .Select(day => new DailySalesDto(day, daily.GetValueOrDefault(day)))
            .ToList();

        return new ShopOrderStatsDto(newOrders, preparing, outForDelivery, delivered, total, week);
    }
}