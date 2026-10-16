using Phloem.Application.Features.PriceRanges.Dtos;
using Phloem.Domain.Entities;
using Phloem.Domain.Enums;

namespace Phloem.Application.Features.DishCatalogs.Dtos;

/// <summary>Admin → dish names list: each name with its price range in every country.</summary>
public sealed record AdminDishCatalogDto(
    Guid Id,
    string Name,
    DishCategory Category,
    CatalogGroup Group,
    IReadOnlyList<DishPriceRangeDto> PriceRanges)
{
    public static AdminDishCatalogDto From(DishCatalog item) => new(
        item.Id,
        item.Name,
        item.Category,
        item.Group,
        item.PriceRanges
            .OrderBy(r => r.Country)
            .Select(DishPriceRangeDto.From)
            .ToList());
}