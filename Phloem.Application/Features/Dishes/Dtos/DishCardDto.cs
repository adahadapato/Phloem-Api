using Phloem.Domain.Entities;
using Phloem.Domain.Enums;
using Phloem.Domain.Interfaces;

namespace Phloem.Application.Features.Dishes.Dtos;

/// <summary>
/// One dish card on Home and the Food listing: "Jollof Rice & Chicken · Mama Ama's Kitchen · GHS 45.00".
/// VendorId is only for "View shop" / basket rules, never shown.
/// </summary>
public sealed record DishCardDto(
    Guid Id,
    string Name,
    DishCategory Category,
    string? Description,
    decimal Price,
    Currency Currency,
    Country Country,
    decimal Rating,
    int RatingCount,
    string? ImageUrl,
    Guid VendorId,
    string VendorName,
    bool IsVerifiedShop)
{
    public static DishCardDto From(Dish d, IFileStorage storage)
    {
        var file = d.PrimaryPhoto?.File;
        var imagePath = file?.ThumbnailPath ?? file?.Path;   // cards use the 400×300 thumbnail

        return new DishCardDto(
            d.Id,
            d.Name,
            d.Category,
            d.Description,
            d.Price,
            d.Currency,
            d.Country,
            d.Rating,
            d.RatingCount,
            imagePath is null ? null : storage.GetPublicUrl(imagePath),
            d.VendorId,
            d.Vendor.Name,
            d.Vendor.IsVerified);
    }
}