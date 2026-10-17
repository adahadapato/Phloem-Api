using MediatR;
using Phloem.Domain.Enums;
using Phloem.Domain.Shared.Results;

namespace Phloem.Application.Features.Shipments.Commands;

/// <summary>
/// Staff move a paid parcel along its journey: InTransit, ArrivedInDestination, OutForDelivery or
/// Delivered. One command for all four (same screen, same shape); the Domain only allows the next step.
/// </summary>
/// <param name="TrackingNumber">The tracking number.</param>
/// <param name="NewStatus">One of the four journey statuses.</param>
/// <param name="Note">Optional timeline detail, e.g. "Flight BA078" or "Signed for by K. Boateng".</param>
public sealed record UpdateShipmentProgressCommand(string TrackingNumber, ShipmentStatus NewStatus, string? Note)
    : IRequest<Result>;
