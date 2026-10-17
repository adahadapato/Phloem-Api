using MediatR;
using Phloem.Application.Features.Orders.Dtos;
using Phloem.Domain.Enums;
using Phloem.Domain.Shared.Results;

namespace Phloem.Application.Features.Orders.Queries;

/// <summary>
/// Seller → "Shop orders", newest first. Never includes unpaid orders. Never cached.
/// </summary>
/// <param name="Status">Only this status (e.g. Paid = "New orders"); null = all (except unpaid).</param>
/// <param name="Page">1-based page number.</param>
/// <param name="PageSize">Orders per page.</param>
public sealed record GetShopOrdersQuery(OrderStatus? Status, int Page = 1, int PageSize = 20)
    : IRequest<Result<PagedResult<ShopOrderSummaryDto>>>;