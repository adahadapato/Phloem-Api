using MediatR;
using Phloem.Application.Features.Deliveries.Common;
using Phloem.Application.Features.Shipments.Dtos;
using Phloem.Domain.Interfaces;
using Phloem.Domain.Shared.Errors;
using Phloem.Domain.Shared.Results;

namespace Phloem.Application.Features.Shipments.Queries;

/// <summary>Handles <see cref="GetMyShipmentQuery"/>.</summary>
public sealed class GetMyShipmentQueryHandler(
    ICurrentUser currentUser,
    IShipmentRepository shipments,
    IShipmentRouteRepository routes,
    DeliveryPhotos photos)
    : IRequestHandler<GetMyShipmentQuery, Result<ShipmentDetailsDto>>
{
    /// <summary>The details, if the caller is the sender.</summary>
    /// <param name="query">The query.</param>
    /// <param name="ct">Cancels the database calls.</param>
    /// <returns>The details; or <see cref="ShipmentErrors.NotFound"/> (also for other people's parcels).</returns>
    /// <exception cref="InvalidOperationException">Called without a signed-in user (endpoint misconfigured).</exception>
    public async Task<Result<ShipmentDetailsDto>> Handle(GetMyShipmentQuery query, CancellationToken ct)
    {
        var userId = currentUser.UserId
            ?? throw new InvalidOperationException("This action requires a signed-in user.");

        var shipment = await shipments.GetByTrackingNumberAsync(query.TrackingNumber.Trim(), ct);
        if (shipment is null || shipment.SenderId != userId) return ShipmentErrors.NotFound;

        var route = await routes.GetByIdAsync(shipment.ShipmentRouteId, ct);
        var proof = await photos.ToDtoAsync(shipment.DeliveryProof, shipment.CourierName, ct);
        return ShipmentDetailsDto.From(shipment, route, proof);
    }
}
