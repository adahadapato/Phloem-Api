using MediatR;
using Phloem.Domain.Enums;
using Phloem.Domain.Shared.Results;

namespace Phloem.Application.Features.Shipments.Commands;

/// <summary>
/// The parcel's courier marks it delivered, with a photo of where they left it (OutForDelivery → Delivered). The
/// timeline says how it was handed over and the sender is emailed.
/// </summary>
/// <param name="TrackingNumber">The parcel.</param>
/// <param name="Photo">The photo (JPEG, PNG, WebP or HEIC); always required.</param>
/// <param name="PhotoSizeBytes">Its size, checked before reading it.</param>
/// <param name="Handover">How it was handed over.</param>
/// <param name="ReceivedBy">Who took it; required when left with a neighbour.</param>
/// <param name="Note">An optional note for the sender.</param>
public sealed record MarkShipmentDeliveredCommand(
    string TrackingNumber,
    Stream Photo,
    long PhotoSizeBytes,
    HandoverMethod Handover,
    string? ReceivedBy,
    string? Note) : IRequest<Result>;
