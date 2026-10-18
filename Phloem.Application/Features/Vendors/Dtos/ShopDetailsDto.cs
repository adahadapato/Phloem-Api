using Phloem.Domain.Entities;
using Phloem.Domain.Enums;
using Phloem.Domain.Interfaces;

namespace Phloem.Application.Features.Vendors.Dtos;

/// <summary>
/// The top of a shop's page: "Mama Ama's Kitchen ✓ · ★ 4.8 (120) · 12 dishes · Delivery GHS 15.00".
/// The menu below it comes from <c>GET /api/dishes?shopId=…</c>.
/// </summary>
/// <remarks>The business email and phone are NOT here: buyers contact shops through their orders.</remarks>
public sealed record ShopDetailsDto(
    Guid Id,
    string Name,
    Country Country,
    Currency Currency,
    bool IsVerified,
    decimal Rating,
    int RatingCount,
    int DishCount,
    decimal? DeliveryFee,
    string? LogoUrl,
    string? CoverPhotoUrl,
    DateTime? OnPhloemSince)
{
    /// <summary>Builds the shop page header.</summary>
    public static ShopDetailsDto From(Vendor v, int dishCount, IFileStorage storage) => new(
        v.Id,
        v.Name,
        v.Country,
        v.Currency,
        v.IsVerified,
        v.Rating,
        v.RatingCount,
        dishCount,
        v.DeliveryFee,
        v.Logo is null ? null : storage.GetPublicUrl(v.Logo.Path),
        v.CoverPhoto is null ? null : storage.GetPublicUrl(v.CoverPhoto.Path),
        v.ApprovedAtUtc);
}
