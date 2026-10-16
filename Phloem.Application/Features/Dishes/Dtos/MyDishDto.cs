using Phloem.Domain.Entities;
using Phloem.Domain.Enums;
using Phloem.Domain.Interfaces;

namespace Phloem.Application.Features.Dishes.Dtos;

/// <summary>
/// Seller dashboard row + Dish editor: everything the seller needs to see and edit,
/// including the allowed price range for their country and whether the dish needs a new price.
/// CatalogName + Detail let the editor show the picked name and detail chip separately.
/// </summary>
public sealed record MyDishDto(
    Guid Id,
    Guid DishCatalogId,
    string CatalogName,
    string? Detail,
    string Name,
    DishCategory Category,
    decimal Price,
    Currency Currency,
    decimal? AllowedMinPrice,
    decimal? AllowedMaxPrice,
    bool NeedsPriceUpdate,
    string? Description,
    bool IsAvailable,
    decimal Rating,
    int RatingCount,
    IReadOnlyList<DishPhotoDto> Photos)
{
    public static MyDishDto From(Dish d, IFileStorage storage)
    {
        var range = d.DishCatalog.PriceRangeFor(d.Country);

        return new MyDishDto(
            d.Id,
            d.DishCatalogId,
            d.DishCatalog.Name,
            d.Detail,
            d.Name,
            d.Category,
            d.Price,
            d.Currency,
            range?.MinPrice,
            range?.MaxPrice,
            d.NeedsPriceUpdate,
            d.Description,
            d.IsAvailable,
            d.Rating,
            d.RatingCount,
            d.Photos.OrderBy(p => p.DisplayOrder).Select(p => DishPhotoDto.From(p, storage)).ToList());
    }
}