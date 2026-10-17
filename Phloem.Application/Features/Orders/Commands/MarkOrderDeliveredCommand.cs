using MediatR;
using Phloem.Domain.Shared.Results;

namespace Phloem.Application.Features.Orders.Commands;

/// <summary>
/// Seller marks the order delivered (OutForDelivery → Delivered). The commission is worked out and
/// the seller's earnings are added to their balance, in the same save.
/// </summary>
/// <param name="OrderNumber">The order.</param>
public sealed record MarkOrderDeliveredCommand(string OrderNumber) : IRequest<Result>;