using MediatR;
using Phloem.Application.Features.Deliveries.Common;
using Phloem.Domain.Enums;
using Phloem.Domain.Interfaces;
using Phloem.Domain.Shared.Errors;
using Phloem.Domain.Shared.Results;
using Phloem.Domain.ValueObjects;

namespace Phloem.Application.Features.Shipments.Commands;

/// <summary>Handles <see cref="MarkShipmentDeliveredCommand"/>.</summary>
public sealed class MarkShipmentDeliveredCommandHandler(
    ICurrentUser currentUser,
    IShipmentRepository shipments,
    DeliveryPhotos photos,
    DeliveryEmails emails,
    IUnitOfWork unitOfWork,
    TimeProvider time)
    : IRequestHandler<MarkShipmentDeliveredCommand, Result>
{
    /// <summary>Only the parcel's courier: saves the photo (private), marks delivered, saves; the photo is removed
    /// again if anything is refused.</summary>
    /// <returns>Success; or <see cref="DeliveryErrors.NotYourDelivery"/> (also for parcels that don't exist),
    /// <see cref="ShipmentErrors.InvalidStatusChange"/>, <see cref="DeliveryErrors.SayWhichNeighbour"/>, an
    /// <c>ImageErrors</c> error.</returns>
    /// <exception cref="InvalidOperationException">Called without a signed-in user (endpoint misconfigured).</exception>
    public async Task<Result> Handle(MarkShipmentDeliveredCommand command, CancellationToken ct)
    {
        var userId = currentUser.UserId
            ?? throw new InvalidOperationException("This action requires a signed-in user.");

        var shipment = await shipments.GetByTrackingNumberAsync(command.TrackingNumber.Trim(), ct);
        if (shipment is null || shipment.CourierId != userId) return DeliveryErrors.NotYourDelivery;
        if (shipment.Status != ShipmentStatus.OutForDelivery)
            return ShipmentErrors.InvalidStatusChange(shipment.Status, ShipmentStatus.Delivered);

        var saved = await photos.SaveAsync(command.Photo, "parcels", shipment.TrackingNumber, ct);
        if (saved.IsFailure) return saved.Error;

        var proof = ProofOfDelivery.Create(saved.Value, command.Handover, command.ReceivedBy, command.Note, userId,
                                           time.GetUtcNow().UtcDateTime);
        var delivered = proof.IsFailure ? Result.Failure(proof.Error) : shipment.MarkDelivered(proof.Value);
        if (delivered.IsFailure)
        {
            await photos.DeleteAsync(saved.Value, ct);
            return delivered;
        }

        await unitOfWork.SaveChangesAsync(ct);

        await emails.ParcelDeliveredAsync(shipment, ct);
        return Result.Success();
    }
}
