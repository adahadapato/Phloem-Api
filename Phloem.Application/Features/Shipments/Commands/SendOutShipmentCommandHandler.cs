using MediatR;
using Phloem.Application.Features.Deliveries.Common;
using Phloem.Domain.Interfaces;
using Phloem.Domain.Shared.Errors;
using Phloem.Domain.Shared.Results;

namespace Phloem.Application.Features.Shipments.Commands;

/// <summary>Handles <see cref="SendOutShipmentCommand"/>.</summary>
public sealed class SendOutShipmentCommandHandler(
    IShipmentRepository shipments,
    CourierDirectory couriers,
    DeliveryEmails emails,
    IUnitOfWork unitOfWork,
    TimeProvider time)
    : IRequestHandler<SendOutShipmentCommand, Result>
{
    /// <returns>Success; or <see cref="ShipmentErrors.NotFound"/>, <see cref="DeliveryErrors.CourierNotFound"/>,
    /// <see cref="DeliveryErrors.CourierInAnotherCountry"/>, <see cref="ShipmentErrors.InvalidStatusChange"/>.</returns>
    public async Task<Result> Handle(SendOutShipmentCommand command, CancellationToken ct)
    {
        var shipment = await shipments.GetByTrackingNumberAsync(command.TrackingNumber.Trim(), ct);
        if (shipment is null) return ShipmentErrors.NotFound;

        var courier = await couriers.FindAsync(command.CourierId, shipment.ToCountry, ct);
        if (courier.IsFailure) return courier.Error;

        var moved = shipment.MarkOutForDelivery(courier.Value.Id, courier.Value.GetFullName(),
                                                time.GetUtcNow().UtcDateTime, command.Note);
        if (moved.IsFailure) return moved;

        await unitOfWork.SaveChangesAsync(ct);

        await emails.NewDeliveryAsync(courier.Value.Id, shipment.TrackingNumber, "A parcel",
                                      $"{shipment.RecipientAddressLine}, {shipment.RecipientCity}", ct);
        return Result.Success();
    }
}
