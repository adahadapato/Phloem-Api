using MediatR;
using Phloem.Application.Features.Deliveries.Common;
using Phloem.Domain.Interfaces;
using Phloem.Domain.Shared.Errors;
using Phloem.Domain.Shared.Results;

namespace Phloem.Application.Features.Shipments.Commands;

/// <summary>Handles <see cref="ChangeShipmentCourierCommand"/>.</summary>
public sealed class ChangeShipmentCourierCommandHandler(
    IShipmentRepository shipments,
    CourierDirectory couriers,
    DeliveryEmails emails,
    IUnitOfWork unitOfWork,
    TimeProvider time)
    : IRequestHandler<ChangeShipmentCourierCommand, Result>
{
    /// <returns>Success; or <see cref="ShipmentErrors.NotFound"/>, <see cref="DeliveryErrors.CourierNotFound"/>,
    /// <see cref="DeliveryErrors.CourierInAnotherCountry"/>, <see cref="DeliveryErrors.NotOutForDelivery"/>.</returns>
    public async Task<Result> Handle(ChangeShipmentCourierCommand command, CancellationToken ct)
    {
        var shipment = await shipments.GetByTrackingNumberAsync(command.TrackingNumber.Trim(), ct);
        if (shipment is null) return ShipmentErrors.NotFound;

        var courier = await couriers.FindAsync(command.CourierId, shipment.ToCountry, ct);
        if (courier.IsFailure) return courier.Error;

        var previous = shipment.CourierId;
        var changed = shipment.ChangeCourier(courier.Value.Id, courier.Value.GetFullName(),
                                             time.GetUtcNow().UtcDateTime);
        if (changed.IsFailure) return changed;

        await unitOfWork.SaveChangesAsync(ct);

        if (courier.Value.Id != previous)
            await emails.NewDeliveryAsync(courier.Value.Id, shipment.TrackingNumber, "A parcel",
                                          $"{shipment.RecipientAddressLine}, {shipment.RecipientCity}", ct);
        return Result.Success();
    }
}
