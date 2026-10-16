using Phloem.Domain.Entities;
using Phloem.Domain.Enums;

namespace Phloem.Application.Features.DishCatalogs.Dtos;

/// <summary>
/// One name in the Add / Edit dish picker, with the allowed price range for the seller's country.
/// HasPriceRange = false → the app shows it greyed out ("price not set yet"): it can't be sold yet.
/// </summary>
public sealed record DishCatalogItemDto(
    Guid Id,
    string Name,
    DishCategory Category,
    CatalogGroup Group,
    bool HasPriceRange,
    decimal? MinPrice,
    decimal? MaxPrice,
    Currency? Currency)
{
    public static DishCatalogItemDto From(DishCatalog item, Country country)
    {
        var range = item.PriceRangeFor(country);

        return new DishCatalogItemDto(
            item.Id,
            item.Name,
            item.Category,
            item.Group,
            range is not null,
            range?.MinPrice,
            range?.MaxPrice,
            range?.Currency);
    }
}