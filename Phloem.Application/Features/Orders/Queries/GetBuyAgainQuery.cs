using MediatR;
using Phloem.Application.Features.Orders.Dtos;
using Phloem.Domain.Shared.Results;

namespace Phloem.Application.Features.Orders.Queries;

/// <summary>
/// "Buy again" on any of the customer's orders: which dishes can go back in the basket.
/// The app then opens Checkout, which prices them at today's prices.
/// </summary>
/// <param name="OrderNumber">The old order.</param>
public sealed record GetBuyAgainQuery(string OrderNumber) : IRequest<Result<BuyAgainDto>>;