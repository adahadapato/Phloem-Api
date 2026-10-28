using MediatR;
using Phloem.Domain.Shared.Results;

namespace Phloem.Application.Features.Shipments.Commands;

/// <summary>Staff give a parcel that's out for delivery to another courier (recorded in the timeline).</summary>
/// <param name="TrackingNumber">The parcel.</param>
/// <param name="CourierId">The new courier.</param>
public sealed record ChangeShipmentCourierCommand(string TrackingNumber, Guid CourierId) : IRequest<Result>;
