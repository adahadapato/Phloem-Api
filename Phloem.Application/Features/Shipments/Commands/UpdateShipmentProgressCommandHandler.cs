using MediatR;
using Phloem.Application.Common.Authorization;
using Phloem.Domain.Enums;
using Phloem.Domain.Interfaces;
using Phloem.Domain.Shared.Errors;
using Phloem.Domain.Shared.Results;

namespace Phloem.Application.Features.Shipments.Commands;

/// <summary>Handles <see cref="UpdateShipmentProgressCommand"/>.</summary>
public sealed class UpdateShipmentProgressCommandHandler(
    ICurrentUser currentUser,
    IShipmentRepository shipments,
    IUnitOfWork unitOfWork,
    TimeProvider time)
    : IRequestHandler<UpdateShipmentProgressCommand, Result>
{
    /// <summary>Calls the matching Domain step and saves.</summary>
    /// <param name="command">The command.</param>
    /// <param name="ct">Cancels the database calls.</param>
    /// <returns>Success; or <see cref="ShipmentErrors.NotFound"/>, <see cref="ShipmentErrors.InvalidStatusChange"/>.</returns>
    /// <exception cref="InvalidOperationException">A status the validator should have refused (a bug).</exception>
    public async Task<Result> Handle(UpdateShipmentProgressCommand command, CancellationToken ct)
    {
        var shipment = await shipments.GetByTrackingNumberAsync(command.TrackingNumber.Trim(), ct);
        // Not found, or from and to countries this person doesn't cover: answer the same way
        if (shipment is null || !currentUser.CoversParcel(shipment)) return ShipmentErrors.NotFound;

        var now = time.GetUtcNow().UtcDateTime;

        var moved = command.NewStatus switch
        {
            ShipmentStatus.InTransit => shipment.MarkInTransit(now, command.Note),
            ShipmentStatus.ArrivedInDestination => shipment.MarkArrivedInDestination(now, command.Note),
            _ => throw new InvalidOperationException($"{command.NewStatus} isn't a journey step.")
        };
        if (moved.IsFailure) return moved;

        await unitOfWork.SaveChangesAsync(ct);
        return Result.Success();
    }
}
