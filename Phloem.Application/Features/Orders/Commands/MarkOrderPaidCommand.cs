using MediatR;
using Phloem.Domain.Shared.Results;

namespace Phloem.Application.Features.Orders.Commands;

/// <summary>
/// The payment is confirmed: the order becomes Paid and the seller can now see it.
/// </summary>
/// <remarks>
/// <para>Sent by the payment provider's callback (Paystack / Stripe, coming in the Payments feature) and, until
/// then, by the TEST payment endpoint. It has no signed-in user (callbacks don't), so it never checks ownership:
/// whoever sends it must have checked the order first.</para>
/// <para>Sending it twice with the same reference is fine (callbacks are often sent twice).</para>
/// </remarks>
/// <param name="OrderNumber">The order.</param>
/// <param name="PaymentReference">The provider's reference for the payment (e.g. "TEST-PH-261018-482193-…").</param>
public sealed record MarkOrderPaidCommand(string OrderNumber, string PaymentReference) : IRequest<Result>;
