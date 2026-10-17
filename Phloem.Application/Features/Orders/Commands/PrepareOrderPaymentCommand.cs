using MediatR;
using Phloem.Application.Features.Orders.Dtos;
using Phloem.Domain.Shared.Results;

namespace Phloem.Application.Features.Orders.Commands;

/// <summary>
/// "Pay" tapped on an unpaid order: refreshes prices and the delivery fee to today's, drops
/// out-of-stock dishes, and returns the amount to charge. The actual charge (Paystack / Stripe)
/// and <c>Order.MarkPaid</c> come in the Payments feature.
/// </summary>
/// <param name="OrderNumber">The unpaid order.</param>
public sealed record PrepareOrderPaymentCommand(string OrderNumber) : IRequest<Result<OrderPaymentDto>>;