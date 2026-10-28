using MediatR;
using Phloem.Domain.Shared.Results;

namespace Phloem.Application.Features.Orders.Commands;

/// <summary>
/// Seller sends the order out for delivery with a courier (Preparing → OutForDelivery). The courier is emailed and
/// sees it in "My deliveries".
/// </summary>
/// <param name="OrderNumber">The order.</param>
/// <param name="CourierId">A courier from "Choose a courier"; null (or the seller's own Id) = "I'll deliver it myself".</param>
public sealed record SendOutOrderCommand(string OrderNumber, Guid? CourierId = null) : IRequest<Result>;
