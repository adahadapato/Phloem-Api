using MediatR;
using Phloem.Domain.Shared.Results;

namespace Phloem.Application.Features.Orders.Commands;

/// <summary>
/// The seller declines a new paid order they can't make (e.g. an ingredient ran out): it's cancelled and flagged
/// for a refund, and the buyer is emailed the reason. If the shop has declined more than
/// <see cref="DeclineOrderCommandHandler.DeclinesBeforeTeamAlert"/> orders in 7 days, the team is emailed.
/// </summary>
/// <param name="OrderNumber">The order.</param>
/// <param name="Reason">Why (shown to the buyer).</param>
public sealed record DeclineOrderCommand(string OrderNumber, string Reason) : IRequest<Result>;
