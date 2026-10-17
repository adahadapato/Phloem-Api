using MediatR;
using Phloem.Application.Features.Shipments.Dtos;
using Phloem.Domain.Shared.Results;

namespace Phloem.Application.Features.Shipments.Queries;

/// <summary>The sender's full shipment page. Only the sender can see it.</summary>
/// <param name="TrackingNumber">The tracking number.</param>
public sealed record GetMyShipmentQuery(string TrackingNumber) : IRequest<Result<ShipmentDetailsDto>>;
