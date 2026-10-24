using MediatR;
using Phloem.Application.Features.Orders.Dtos;
using Phloem.Domain.Enums;
using Phloem.Domain.Shared.Results;

namespace Phloem.Application.Features.Orders.Queries;

/// <summary>
/// Orders tab: the signed-in customer's orders (unpaid ones included), newest first, optionally narrowed down by a
/// tab, a start date and a search text. Unpaid orders show today's prices and out-of-stock notes. Never cached.
/// </summary>
/// <param name="Page">1-based page number.</param>
/// <param name="PageSize">Orders per page.</param>
/// <param name="Show">The tab: all, waiting for payment, in progress, completed or cancelled.</param>
/// <param name="From">Only orders placed on or after this day (UTC), e.g. three months ago; null = any time.</param>
/// <param name="Search">Text in the order number, shop name or a dish's name; null = no search.</param>
public sealed record GetMyOrdersQuery(
    int Page = 1,
    int PageSize = 20,
    OrderStatusGroup Show = OrderStatusGroup.All,
    DateOnly? From = null,
    string? Search = null)
    : IRequest<Result<PagedResult<OrderSummaryDto>>>;
