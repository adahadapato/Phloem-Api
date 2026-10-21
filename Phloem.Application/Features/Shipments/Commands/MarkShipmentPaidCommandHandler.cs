using MediatR;
using Microsoft.Extensions.Logging;
using Phloem.Domain.Interfaces;
using Phloem.Domain.Shared.Errors;
using Phloem.Domain.Shared.Results;

namespace Phloem.Application.Features.Shipments.Commands;

/// <summary>Handles <see cref="MarkShipmentPaidCommand"/>.</summary>
public sealed class MarkShipmentPaidCommandHandler(
    IShipmentRepository shipments,
    IUnitOfWork unitOfWork,
    TimeProvider time,
    ILogger<MarkShipmentPaidCommandHandler> logger)
    : IRequestHandler<MarkShipmentPaidCommand, Result>
{
    /// <summary>Marks the shipment paid and saves.</summary>
    /// <returns>Success; or <see cref="ShipmentErrors.NotFound"/>, <see cref="ShipmentErrors.NotReadyForPayment"/>.</returns>
    public async Task<Result> Handle(MarkShipmentPaidCommand command, CancellationToken ct)
    {
        var shipment = await shipments.GetByTrackingNumberAsync(command.TrackingNumber.Trim(), ct);
        if (shipment is null) return ShipmentErrors.NotFound;

        var paid = shipment.MarkPaid(command.PaymentReference, time.GetUtcNow().UtcDateTime);
        if (paid.IsFailure) return paid;

        await unitOfWork.SaveChangesAsync(ct);

        logger.LogInformation("Shipment {TrackingNumber} paid ({PaymentReference}).",
                              shipment.TrackingNumber, command.PaymentReference.Trim());
        return Result.Success();
    }
}
