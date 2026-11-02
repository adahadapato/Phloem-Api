using MediatR;
using Phloem.Application.Common.Authorization;
using Phloem.Application.Features.Deliveries.Common;
using Phloem.Application.Features.Shipments.Dtos;
using Phloem.Domain.Entities;
using Phloem.Domain.Enums;
using Phloem.Domain.Interfaces;
using Phloem.Domain.Shared.Errors;
using Phloem.Domain.Shared.Results;

namespace Phloem.Application.Features.Shipments.Queries;

/// <summary>Handles <see cref="GetStaffShipmentQuery"/>.</summary>
/// <remarks>The details show the sender's and recipient's names, phones and address, so every opening is written to
/// the staff audit log.</remarks>
public sealed class GetStaffShipmentQueryHandler(ICurrentUser currentUser, IShipmentRepository shipments,
                                                 IShipmentRouteRepository routes, DeliveryPhotos photos,
                                                 IStaffAuditRepository audit, IUnitOfWork unitOfWork)
    : IRequestHandler<GetStaffShipmentQuery, Result<ShipmentDetailsDto>>
{
    /// <summary>A shipment's full details, when it's from or to one of the person's countries.</summary>
    /// <param name="query">The query.</param>
    /// <param name="ct">Cancels the database calls.</param>
    /// <returns>The details; or <see cref="ShipmentErrors.NotFound"/>.</returns>
    public async Task<Result<ShipmentDetailsDto>> Handle(GetStaffShipmentQuery query, CancellationToken ct)
    {
        var shipment = await shipments.GetByTrackingNumberAsync(query.TrackingNumber.Trim(), ct);
        if (shipment is null || !currentUser.CoversParcel(shipment)) return ShipmentErrors.NotFound;

        if (currentUser.UserId is { } actorId)
        {
            audit.Add(StaffAuditEntry.Create(actorId, StaffAction.ParcelViewed, $"Parcel {shipment.TrackingNumber}",
                                             shipment.FromCountry, $"To {shipment.ToCountry}"));
            await unitOfWork.SaveChangesAsync(ct);
        }

        var route = await routes.GetByIdAsync(shipment.ShipmentRouteId, ct);
        var proof = await photos.ToDtoAsync(shipment.DeliveryProof, shipment.CourierName, ct);
        return ShipmentDetailsDto.From(shipment, route, proof);
    }
}
