using Phloem.Domain.Entities;
using Phloem.Domain.Enums;

namespace Phloem.Application.Features.DeliveryPricings.Dtos;

/// <summary>Admin → Delivery pricing page: one country's shop fee range and window charges.</summary>
/// <param name="Country">The country.</param>
/// <param name="Currency">The country's currency.</param>
/// <param name="MinShopFee">Lowest fee a shop may charge (inclusive).</param>
/// <param name="MaxShopFee">Highest fee a shop may charge (inclusive).</param>
/// <param name="Windows">Offered delivery windows and their extra charges, in window order.</param>
public sealed record DeliveryPricingDto(
    Country Country,
    Currency Currency,
    decimal MinShopFee,
    decimal MaxShopFee,
    IReadOnlyList<DeliveryWindowChargeDto> Windows)
{
    /// <summary>Maps a country's pricing.</summary>
    /// <param name="pricing">The pricing to map (with its window charges loaded).</param>
    public static DeliveryPricingDto From(DeliveryPricing pricing) => new(
        pricing.Country,
        pricing.Currency,
        pricing.MinShopFee,
        pricing.MaxShopFee,
        pricing.WindowCharges
               .OrderBy(c => c.Window)
               .Select(DeliveryWindowChargeDto.From)
               .ToList());
}