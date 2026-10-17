using MediatR;
using Phloem.Application.Features.Orders.Dtos;
using Phloem.Domain.Shared.Results;

namespace Phloem.Application.Features.Orders.Queries;

/// <summary>
/// Orders tab: the signed-in customer's orders (unpaid ones included), newest first.
/// Unpaid orders show today's prices and out-of-stock notes. Never cached.
/// </summary>
/// <param name="Page">1-based page number.</param>
/// <param name="PageSize">Orders per page.</param>
public sealed record GetMyOrdersQuery(int Page = 1, int PageSize = 20)
    : IRequest<Result<PagedResult<OrderSummaryDto>>>;