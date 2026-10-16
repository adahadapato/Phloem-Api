using MediatR;
using Phloem.Application.Common.Caching;
using Phloem.Domain.Enums;
using Phloem.Domain.Shared.Results;

namespace Phloem.Application.Features.DeliveryPricings.Commands;

/// <summary>
/// Admin sets (or creates) a country's shop delivery-fee range (inclusive).
/// Shops whose own fee is now outside it are moved to the nearest limit and emailed. No shop is blocked.
/// </summary>
/// <param name="Country">The country.</param>
/// <param name="MinShopFee">Lowest fee a shop may charge; 0 or more.</param>
/// <param name="MaxShopFee">Highest fee a shop may charge; at least the minimum.</param>
/// <returns>How many shops had their fee adjusted (so the admin sees the effect).</returns>
[InvalidatesCache(CacheTags.DeliveryPricing)]
public sealed record SetDeliveryPricingCommand(Country Country, decimal MinShopFee, decimal MaxShopFee)
    : IRequest<Result<int>>;