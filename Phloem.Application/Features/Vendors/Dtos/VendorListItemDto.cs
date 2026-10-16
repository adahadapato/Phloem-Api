using Phloem.Domain.Entities;
using Phloem.Domain.Enums;
using Phloem.Domain.Interfaces;

namespace Phloem.Application.Features.Vendors.Dtos;

/// <summary>One card on "Meet local vendors" and Home → Top sellers. The tagline is decided by the app.</summary>
public sealed record VendorListItemDto(
    Guid Id,
    string Name,
    Country Country,
    bool IsVerified,
    decimal Rating,
    int RatingCount,
    int DishCount,
    string? LogoUrl,
    string? CoverPhotoUrl)
{
    public static VendorListItemDto From(Vendor v, int dishCount, IFileStorage storage) => new(
        v.Id,
        v.Name,
        v.Country,
        v.IsVerified,
        v.Rating,
        v.RatingCount,
        dishCount,
        v.Logo is null ? null : storage.GetPublicUrl(v.Logo.Path),
        v.CoverPhoto is null ? null : storage.GetPublicUrl(v.CoverPhoto.Path));
}