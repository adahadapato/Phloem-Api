using MediatR;
using Phloem.Application.Common.Caching;
using Phloem.Domain.Enums;
using Phloem.Domain.Shared.Results;

namespace Phloem.Application.Features.DeliveryPricings.Commands;

/// <summary>
/// Admin offers a delivery window in a country, or changes its extra charge (0 = no extra).
/// The country's pricing must exist first (<see cref="SetDeliveryPricingCommand"/>).
/// </summary>
/// <param name="Country">The country.</param>
/// <param name="Window">The delivery window.</param>
/// <param name="ExtraCharge">Amount added to each shop's fee for this window; 0 or more.</param>
[InvalidatesCache(CacheTags.DeliveryPricing)]
public sealed record SetDeliveryWindowChargeCommand(Country Country, DeliveryWindow Window, decimal ExtraCharge)
    : IRequest<Result>;