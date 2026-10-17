using MediatR;
using Phloem.Application.Common.Caching;
using Phloem.Domain.Enums;
using Phloem.Domain.Shared.Results;

namespace Phloem.Application.Features.ShipmentRoutes.Commands;

/// <summary>
/// Admin switches a route on (customers can book) or off (no new bookings; existing parcels carry on).
/// One command with a flag, because it's one toggle on the admin screen.
/// </summary>
/// <param name="FromCountry">Origin.</param>
/// <param name="ToCountry">Destination.</param>
/// <param name="IsActive">True = switch on (needs at least one rate); false = switch off.</param>
[InvalidatesCache(CacheTags.ShipmentRoutes)]
public sealed record SetShipmentRouteActiveCommand(Country FromCountry, Country ToCountry, bool IsActive)
    : IRequest<Result>;
