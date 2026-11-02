using MediatR;
using Phloem.Application.Features.Orders.Dtos;
using Phloem.Domain.Enums;
using Phloem.Domain.Shared.Results;

namespace Phloem.Application.Features.Orders.Queries;

/// <summary>
/// Admin → Orders: every order delivered to the person's countries, newest first, with optional filters.
/// Permission <c>orders.view</c>. Read-only, not cached.
/// </summary>
/// <param name="Country">Only orders delivered to this country; null = all the person covers.</param>
/// <param name="ShopId">Only this shop's orders.</param>
/// <param name="Status">Only this status.</param>
/// <param name="From">Placed on or after this day (UTC).</param>
/// <param name="To">Placed on or before this day (UTC, the whole day counts).</param>
/// <param name="Search">Order number (or its start), shop name, or customer name.</param>
/// <param name="Page">1-based page number.</param>
/// <param name="PageSize">Orders per page.</param>
public sealed record GetAdminOrdersQuery(Country? Country, Guid? ShopId, OrderStatus? Status,
                                         DateOnly? From, DateOnly? To, string? Search,
                                         int Page = 1, int PageSize = 20)
    : IRequest<Result<PagedResult<AdminOrderSummaryDto>>>;
