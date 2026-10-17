using MediatR;
using Phloem.Domain.Shared.Results;

namespace Phloem.Application.Features.Orders.Commands;

/// <summary>
/// Customer cancels an order: unpaid any time; paid within 30 minutes of payment and before the
/// shop starts. A paid order that is cancelled is flagged for a refund (Payments feature).
/// </summary>
/// <param name="OrderNumber">The order.</param>
public sealed record CancelOrderCommand(string OrderNumber) : IRequest<Result>;