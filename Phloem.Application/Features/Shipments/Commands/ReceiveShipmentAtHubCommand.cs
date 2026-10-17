using MediatR;
using Phloem.Domain.Shared.Results;

namespace Phloem.Application.Features.Shipments.Commands;

/// <summary>
/// Staff at the hub: the parcel was dropped off and weighed. Sets the FINAL price and transit days,
/// then emails the sender to pay. Returns the final price for the counter screen.
/// </summary>
/// <param name="TrackingNumber">The tracking number.</param>
/// <param name="WeighedWeightKg">The measured weight in kg.</param>
public sealed record ReceiveShipmentAtHubCommand(string TrackingNumber, decimal WeighedWeightKg)
    : IRequest<Result<decimal>>;
