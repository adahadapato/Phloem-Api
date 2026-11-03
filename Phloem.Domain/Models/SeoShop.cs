using Phloem.Domain.Enums;

namespace Phloem.Domain.Models;

/// <summary>A live shop, as SEO needs it (list of every shop buyers can see).</summary>
/// <param name="Id">The shop.</param>
/// <param name="Name">Its name.</param>
/// <param name="Country">Its country.</param>
/// <param name="IsVerified">Has the ✓ badge.</param>
/// <param name="Rating">Average stars.</param>
/// <param name="RatingCount">How many ratings.</param>
/// <param name="ImagePath">Cover photo, else logo (stored path); null if neither.</param>
/// <param name="ChangedAtUtc">Last change (for the sitemap).</param>
public sealed record SeoShop(
    Guid Id,
    string Name,
    Country Country,
    bool IsVerified,
    decimal Rating,
    int RatingCount,
    string? ImagePath,
    DateTime ChangedAtUtc);
