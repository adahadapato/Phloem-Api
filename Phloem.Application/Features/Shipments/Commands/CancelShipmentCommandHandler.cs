using MediatR;
using Phloem.Domain.Interfaces;
using Phloem.Domain.Shared.Errors;
using Phloem.Domain.Shared.Results;

namespace Phloem.Application.Features.Shipments.Commands;

/// <summary>Handles <see cref="CancelShipmentCommand"/>.</summary>
public sealed class CancelShipmentCommandHandler(
    ICurrentUser currentUser,
    IShipmentRepository shipments,
    IUnitOfWork unitOfWork,
    TimeProvider time)
    : IRequestHandler<CancelShipmentCommand, Result>
{
    /// <summary>Cancels the sender's own shipment.</summary>
    /// <param name="command">The command.</param>
    /// <param name="ct">Cancels the database calls.</param>
    /// <returns>Success; or <see cref="ShipmentErrors.NotFound"/>, <see cref="ShipmentErrors.CannotCancel"/>.</returns>
    /// <exception cref="InvalidOperationException">Called without a signed-in user (endpoint misconfigured).</exception>
    public async Task<Result> Handle(CancelShipmentCommand command, CancellationToken ct)
    {
        var userId = currentUser.UserId
            ?? throw new InvalidOperationException("This action requires a signed-in user.");

        var shipment = await shipments.GetByTrackingNumberAsync(command.TrackingNumber.Trim(), ct);
        if (shipment is null || shipment.SenderId != userId) return ShipmentErrors.NotFound;

        var cancelled = shipment.Cancel(time.GetUtcNow().UtcDateTime);
        if (cancelled.IsFailure) return cancelled;

        await unitOfWork.SaveChangesAsync(ct);
        return Result.Success();
    }
}
