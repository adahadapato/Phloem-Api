using Phloem.Domain.Entities;
using Phloem.Domain.Enums;
using Phloem.Domain.Interfaces;

namespace Phloem.Application.Features.Dishes.Dtos;

/// <summary>
/// A dish's page: every photo, the description, and the shop it comes from.
/// CanBeOrdered = false → the app greys out "Add to basket" (sold out today, or waiting for a new price).
/// </summary>
public sealed record DishDetailsDto(
    Guid Id,
    string Name,
    DishCategory Category,
    string? Description,
    decimal Price,
    Currency Currency,
    Country Country,
    decimal Rating,
    int RatingCount,
    bool CanBeOrdered,
    IReadOnlyList<DishPhotoDto> Photos,
    Guid ShopId,
    string ShopName,
    bool IsVerifiedShop,
    string? ShopLogoUrl)
{
    /// <summary>Builds the dish page (the dish must be loaded with its photos and shop).</summary>
    public static DishDetailsDto From(Dish d, IFileStorage storage) => new(
        d.Id,
        d.Name,
        d.Category,
        d.Description,
        d.Price,
        d.Currency,
        d.Country,
        d.Rating,
        d.RatingCount,
        d.CanBeOrdered,
        d.Photos.OrderBy(p => p.DisplayOrder).Select(p => DishPhotoDto.From(p, storage)).ToList(),
        d.VendorId,
        d.Vendor.Name,
        d.Vendor.IsVerified,
        d.Vendor.Logo is null ? null : storage.GetPublicUrl(d.Vendor.Logo.Path));
}
