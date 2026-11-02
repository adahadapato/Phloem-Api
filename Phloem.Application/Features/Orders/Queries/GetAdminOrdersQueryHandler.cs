using MediatR;
using Phloem.Application.Common.Authorization;
using Phloem.Application.Features.Orders.Dtos;
using Phloem.Domain.Interfaces;
using Phloem.Domain.Models;
using Phloem.Domain.Shared.Results;

namespace Phloem.Application.Features.Orders.Queries;

/// <summary>Handles <see cref="GetAdminOrdersQuery"/>.</summary>
public sealed class GetAdminOrdersQueryHandler(
    ICurrentUser currentUser,
    IOrderRepository orders,
    IUserProfileRepository profiles)
    : IRequestHandler<GetAdminOrdersQuery, Result<PagedResult<AdminOrderSummaryDto>>>
{
    private const string UnknownUser = "Unknown user";

    /// <summary>One filtered page of orders in the person's countries, with customer names (one extra query).</summary>
    /// <param name="query">The query.</param>
    /// <param name="ct">Cancels the database calls.</param>
    /// <returns>The page. Asking for a country the person doesn't cover gives an empty page.</returns>
    public async Task<Result<PagedResult<AdminOrderSummaryDto>>> Handle(GetAdminOrdersQuery query,
                                                                        CancellationToken ct)
    {
        var page = new PageRequest { Page = query.Page, PageSize = query.PageSize };

        if (query.Country is { } asked && !currentUser.CoversCountry(asked))
            return PagedResult<AdminOrderSummaryDto>.Empty(page.Page, page.PageSize);

        var filter = new AdminOrderFilter(
            query.Country,
            query.ShopId,
            query.Status,
            query.From?.ToDateTime(TimeOnly.MinValue, DateTimeKind.Utc),
            query.To?.AddDays(1).ToDateTime(TimeOnly.MinValue, DateTimeKind.Utc),
            string.IsNullOrWhiteSpace(query.Search) ? null : query.Search.Trim());

        var paged = await orders.GetAdminPagedAsync(filter, currentUser.CountryLimit(), page, ct);

        var names = await profiles.GetFullNamesAsync(paged.Items.Select(o => o.CustomerId).Distinct().ToList(), ct);

        var items = paged.Items.Select(o => new AdminOrderSummaryDto(
            o.OrderNumber,
            o.VendorId,
            o.VendorName,
            names.GetValueOrDefault(o.CustomerId, UnknownUser),
            o.DeliveryCountry,
            o.Status,
            o.Total,
            o.Currency,
            o.Lines.Sum(l => l.Quantity),
            o.PlacedAtUtc,
            o.CourierName,
            o.NeedsRefund)).ToList();

        return new PagedResult<AdminOrderSummaryDto>(items, paged.TotalCount, paged.Page, paged.PageSize);
    }
}
