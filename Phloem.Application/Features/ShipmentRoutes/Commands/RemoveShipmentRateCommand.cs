using MediatR;
using Phloem.Application.Common.Caching;
using Phloem.Domain.Enums;
using Phloem.Domain.Shared.Results;

namespace Phloem.Application.Features.ShipmentRoutes.Commands;

/// <summary>
/// Admin stops offering a speed on a route. Parcels already booked at that speed can't be weighed
/// until the rate is set again (staff will see "speed not offered").
/// </summary>
/// <param name="FromCountry">Origin.</param>
/// <param name="ToCountry">Destination.</param>
/// <param name="Speed">The speed to remove.</param>
[InvalidatesCache(CacheTags.ShipmentRoutes)]
public sealed record RemoveShipmentRateCommand(Country FromCountry, Country ToCountry, DeliverySpeed Speed)
    : IRequest<Result>;
