using MediatR;
using Phloem.Application.Features.Deliveries.Common;
using Phloem.Application.Features.Shipments.Dtos;
using Phloem.Domain.Interfaces;
using Phloem.Domain.Shared.Errors;
using Phloem.Domain.Shared.Results;

namespace Phloem.Application.Features.Shipments.Queries;

/// <summary>Handles <see cref="GetStaffShipmentQuery"/>.</summary>
public sealed class GetStaffShipmentQueryHandler(IShipmentRepository shipments, IShipmentRouteRepository routes,
                                          DeliveryPhotos photos)
    : IRequestHandler<GetStaffShipmentQuery, Result<ShipmentDetailsDto>>
{
    /// <summary>Any shipment's full details.</summary>
    /// <param name="query">The query.</param>
    /// <param name="ct">Cancels the database calls.</param>
    /// <returns>The details; or <see cref="ShipmentErrors.NotFound"/>.</returns>
    public async Task<Result<ShipmentDetailsDto>> Handle(GetStaffShipmentQuery query, CancellationToken ct)
    {
        var shipment = await shipments.GetByTrackingNumberAsync(query.TrackingNumber.Trim(), ct);
        if (shipment is null) return ShipmentErrors.NotFound;

        var route = await routes.GetByIdAsync(shipment.ShipmentRouteId, ct);
        var proof = await photos.ToDtoAsync(shipment.DeliveryProof, shipment.CourierName, ct);
        return ShipmentDetailsDto.From(shipment, route, proof);
    }
}
