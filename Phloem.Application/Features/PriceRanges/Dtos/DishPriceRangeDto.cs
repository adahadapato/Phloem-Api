using Phloem.Domain.Entities;
using Phloem.Domain.Enums;

namespace Phloem.Application.Features.PriceRanges.Dtos;

/// <summary>
/// One allowed price range (inclusive) for a dish name in one country.
/// Reused by any screen that shows or edits ranges.
/// </summary>
public sealed record DishPriceRangeDto(
    Guid Id,
    Guid DishCatalogId,
    Country Country,
    Currency Currency,
    decimal MinPrice,
    decimal MaxPrice)
{
    public static DishPriceRangeDto From(DishPriceRange range) => new(
        range.Id,
        range.DishCatalogId,
        range.Country,
        range.Currency,
        range.MinPrice,
        range.MaxPrice);
}