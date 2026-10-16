using MediatR;
using Phloem.Domain.Shared.Results;

namespace Phloem.Application.Features.DeliveryPricings.Commands;

/// <summary>Shop settings → Delivery fee: the seller sets their own fee inside the country's range.</summary>
/// <param name="Fee">The new fee; must be inside the admin's range (inclusive).</param>
public sealed record SetMyDeliveryFeeCommand(decimal Fee) : IRequest<Result>;