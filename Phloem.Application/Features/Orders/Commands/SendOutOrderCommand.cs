using MediatR;
using Phloem.Domain.Shared.Results;

namespace Phloem.Application.Features.Orders.Commands;

/// <summary>Seller sends the order out for delivery (Preparing → OutForDelivery).</summary>
/// <param name="OrderNumber">The order.</param>
public sealed record SendOutOrderCommand(string OrderNumber) : IRequest<Result>;