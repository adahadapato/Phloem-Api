using MediatR;
using Phloem.Domain.Shared.Results;

namespace Phloem.Application.Features.Orders.Commands;

/// <summary>
/// Seller gives an order that's already out for delivery to another courier (or takes it back themselves), e.g. when
/// the first courier can't make it. The new courier is emailed.
/// </summary>
/// <param name="OrderNumber">The order.</param>
/// <param name="CourierId">The new courier; null (or the seller's own Id) = "I'll deliver it myself".</param>
public sealed record ChangeOrderCourierCommand(string OrderNumber, Guid? CourierId) : IRequest<Result>;
