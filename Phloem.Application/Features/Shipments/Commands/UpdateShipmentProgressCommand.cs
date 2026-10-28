using MediatR;
using Phloem.Domain.Enums;
using Phloem.Domain.Shared.Results;

namespace Phloem.Application.Features.Shipments.Commands;

/// <summary>
/// Staff move a paid parcel along its journey: InTransit or ArrivedInDestination. (Out for delivery is
/// <see cref="SendOutShipmentCommand"/>, with a courier; Delivered is <see cref="MarkShipmentDeliveredCommand"/>, by
/// that courier with a photo.) The Domain only allows the next step.
/// </summary>
/// <param name="TrackingNumber">The tracking number.</param>
/// <param name="NewStatus">InTransit or ArrivedInDestination.</param>
/// <param name="Note">Optional timeline detail, e.g. "Flight BA078" or "Cleared customs".</param>
public sealed record UpdateShipmentProgressCommand(string TrackingNumber, ShipmentStatus NewStatus, string? Note)
    : IRequest<Result>;
