using MediatR;
using Phloem.Application.Features.Orders.Dtos;
using Phloem.Domain.Shared.Results;

namespace Phloem.Application.Features.Orders.Queries;

/// <summary>Seller dashboard: order counts per status and the last 7 days' sales. Not cached.</summary>
public sealed record GetShopOrderStatsQuery : IRequest<Result<ShopOrderStatsDto>>;