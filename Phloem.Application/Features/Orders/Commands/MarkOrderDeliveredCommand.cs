using MediatR;
using Phloem.Domain.Enums;
using Phloem.Domain.Shared.Results;

namespace Phloem.Application.Features.Orders.Commands;

/// <summary>
/// The order's courier marks it delivered, with a photo of where they left it (OutForDelivery → Delivered). The
/// commission is worked out and the shop's earnings are added to its balance, in the same save; the customer is
/// emailed.
/// </summary>
/// <param name="OrderNumber">The order.</param>
/// <param name="Photo">The photo (JPEG, PNG, WebP or HEIC); always required.</param>
/// <param name="PhotoSizeBytes">Its size, checked before reading it.</param>
/// <param name="Handover">How it was handed over.</param>
/// <param name="ReceivedBy">Who took it; required when left with a neighbour.</param>
/// <param name="Note">An optional note for the customer.</param>
public sealed record MarkOrderDeliveredCommand(
    string OrderNumber,
    Stream Photo,
    long PhotoSizeBytes,
    HandoverMethod Handover,
    string? ReceivedBy,
    string? Note) : IRequest<Result>;
