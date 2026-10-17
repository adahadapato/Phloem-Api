using MediatR;
using Phloem.Application.Features.Shipments.Dtos;
using Phloem.Domain.Shared.Results;

namespace Phloem.Application.Features.Shipments.Queries;

/// <summary>Staff: the FULL view of any shipment, e.g. at the hub counter after scanning the number.</summary>
/// <param name="TrackingNumber">The tracking number.</param>
public sealed record GetStaffShipmentQuery(string TrackingNumber) : IRequest<Result<ShipmentDetailsDto>>;
