using MediatR;
using Phloem.Domain.Shared.Results;

namespace Phloem.Application.Features.Shipments.Commands;

/// <summary>
/// Staff hand an arrived parcel to a courier in the destination country (ArrivedInDestination → OutForDelivery). The
/// courier is emailed and sees it in "My deliveries".
/// </summary>
/// <param name="TrackingNumber">The parcel.</param>
/// <param name="CourierId">A courier from "Choose a courier" (GET /api/staff/couriers?country=…).</param>
/// <param name="Note">Optional timeline note; "With our courier, {name}" when blank.</param>
public sealed record SendOutShipmentCommand(string TrackingNumber, Guid CourierId, string? Note) : IRequest<Result>;
