using MediatR;
using Phloem.Domain.Shared.Results;

namespace Phloem.Application.Features.Shipments.Commands;

/// <summary>
/// The sender cancels before the parcel leaves (Booked, ReceivedAtHub or Paid).
/// A paid one is flagged for a refund (Payments feature).
/// </summary>
/// <param name="TrackingNumber">The tracking number.</param>
public sealed record CancelShipmentCommand(string TrackingNumber) : IRequest<Result>;
