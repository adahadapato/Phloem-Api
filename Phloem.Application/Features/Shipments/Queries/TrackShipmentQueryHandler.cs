using MediatR;
using Phloem.Application.Features.Shipments.Dtos;
using Phloem.Domain.Interfaces;
using Phloem.Domain.Shared.Errors;
using Phloem.Domain.Shared.Results;

namespace Phloem.Application.Features.Shipments.Queries;

/// <summary>Handles <see cref="TrackShipmentQuery"/>.</summary>
public sealed class TrackShipmentQueryHandler(IShipmentRepository shipments)
    : IRequestHandler<TrackShipmentQuery, Result<PublicTrackingDto>>
{
    /// <summary>The public view of a shipment.</summary>
    /// <param name="query">The query.</param>
    /// <param name="ct">Cancels the database call.</param>
    /// <returns>The public view; or <see cref="ShipmentErrors.NotFound"/>.</returns>
    public async Task<Result<PublicTrackingDto>> Handle(TrackShipmentQuery query, CancellationToken ct)
    {
        var shipment = await shipments.GetByTrackingNumberAsync(query.TrackingNumber.Trim(), ct);
        if (shipment is null) return ShipmentErrors.NotFound;

        return PublicTrackingDto.From(shipment);
    }
}
