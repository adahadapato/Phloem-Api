using MediatR;
using Phloem.Application.Common.Caching;
using Phloem.Domain.Enums;
using Phloem.Domain.Shared.Results;

namespace Phloem.Application.Features.ShipmentRoutes.Commands;

/// <summary>
/// Admin sets (or changes) one speed's price and transit time on a route. Only affects parcels
/// weighed from now on (the final price is set at weighing).
/// </summary>
/// <param name="FromCountry">Origin.</param>
/// <param name="ToCountry">Destination.</param>
/// <param name="Speed">The speed.</param>
/// <param name="BaseCharge">Fixed amount per parcel (origin currency).</param>
/// <param name="PricePerKg">Amount per chargeable kg.</param>
/// <param name="MinTransitDays">Fewest days in transit.</param>
/// <param name="MaxTransitDays">Most days in transit.</param>
[InvalidatesCache(CacheTags.ShipmentRoutes)]
public sealed record SetShipmentRateCommand(Country FromCountry, Country ToCountry, DeliverySpeed Speed,
                                            decimal BaseCharge, decimal PricePerKg,
                                            int MinTransitDays, int MaxTransitDays) : IRequest<Result>;
