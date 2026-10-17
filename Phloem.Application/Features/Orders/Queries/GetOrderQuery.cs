using MediatR;
using Phloem.Application.Features.Orders.Dtos;
using Phloem.Domain.Shared.Results;

namespace Phloem.Application.Features.Orders.Queries;

/// <summary>
/// Order details page, by order number. Allowed for the customer who placed it, and for the
/// shop it's from once it has been paid. Anyone else gets "not found". Never cached.
/// </summary>
/// <param name="OrderNumber">The order number, e.g. "PH-261016-482193".</param>
public sealed record GetOrderQuery(string OrderNumber) : IRequest<Result<OrderDetailsDto>>;