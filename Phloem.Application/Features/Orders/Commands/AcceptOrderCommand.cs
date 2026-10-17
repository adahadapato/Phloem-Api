using MediatR;
using Phloem.Domain.Shared.Results;

namespace Phloem.Application.Features.Orders.Commands;

/// <summary>Seller accepts a paid order and starts cooking (Paid → Preparing). The customer can no longer cancel.</summary>
/// <param name="OrderNumber">The order.</param>
public sealed record AcceptOrderCommand(string OrderNumber) : IRequest<Result>;