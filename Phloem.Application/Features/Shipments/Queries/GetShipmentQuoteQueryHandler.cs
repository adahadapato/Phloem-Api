using MediatR;
using Phloem.Application.Features.Shipments.Dtos;
using Phloem.Domain.Entities;
using Phloem.Domain.Interfaces;
using Phloem.Domain.Shared.Errors;
using Phloem.Domain.Shared.Results;

namespace Phloem.Application.Features.Shipments.Queries;

/// <summary>Handles <see cref="GetShipmentQuoteQuery"/>.</summary>
public sealed class GetShipmentQuoteQueryHandler(IShipmentRouteRepository routes)
    : IRequestHandler<GetShipmentQuoteQuery, Result<ShipmentQuoteDto>>
{
    /// <summary>Prices the parcel with the route's current rates (the Domain does the maths).</summary>
    /// <param name="query">The query.</param>
    /// <param name="ct">Cancels the database call.</param>
    /// <returns>The quote; or <see cref="ShipmentErrors.RouteNotAvailable"/>, <see cref="ShipmentErrors.SpeedNotOffered"/>.</returns>
    public async Task<Result<ShipmentQuoteDto>> Handle(GetShipmentQuoteQuery query, CancellationToken ct)
    {
        var route = await routes.GetByCountriesAsync(query.FromCountry, query.ToCountry, ct);
        if (route is null) return ShipmentErrors.RouteNotAvailable;

        var price = route.PriceFor(query.Speed, query.EstimatedWeightKg);
        if (price.IsFailure) return price.Error;

        var rate = route.Rates.First(r => r.Speed == query.Speed);   // exists: PriceFor succeeded

        return new ShipmentQuoteDto(
            query.Speed,
            query.EstimatedWeightKg,
            ShipmentRoute.ChargeableWeight(query.EstimatedWeightKg),
            price.Value,
            route.Currency,
            rate.MinTransitDays,
            rate.MaxTransitDays);
    }
}
