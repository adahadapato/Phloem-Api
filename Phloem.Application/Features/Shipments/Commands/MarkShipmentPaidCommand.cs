using MediatR;
using Phloem.Domain.Shared.Results;

namespace Phloem.Application.Features.Shipments.Commands;

/// <summary>
/// The payment for a weighed parcel is confirmed: the shipment becomes Paid and can start its journey.
/// </summary>
/// <remarks>
/// <para>Sent by the payment provider's callback (Paystack / Stripe, coming in the Payments feature) and, until
/// then, by the TEST payment endpoint. It has no signed-in user (callbacks don't), so it never checks ownership:
/// whoever sends it must have checked the shipment first.</para>
/// <para>Sending it twice with the same reference is fine (callbacks are often sent twice).</para>
/// </remarks>
/// <param name="TrackingNumber">The shipment.</param>
/// <param name="PaymentReference">The provider's reference for the payment (e.g. "TEST-PHS…-20261021…").</param>
public sealed record MarkShipmentPaidCommand(string TrackingNumber, string PaymentReference) : IRequest<Result>;
