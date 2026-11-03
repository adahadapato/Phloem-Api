using System.Text.Json;
using MediatR;
using Phloem.Application.Common.Caching;
using Phloem.Application.Features.Seo.Common;
using Phloem.Application.Features.Seo.Dtos;
using Phloem.Domain.Constants;
using Phloem.Domain.Enums;
using Phloem.Domain.Interfaces;
using Phloem.Domain.Shared.Errors;
using Phloem.Domain.Shared.Results;

namespace Phloem.Application.Features.Seo.Queries;

/// <summary>
/// The website asks this for every public page: the title, description, preview picture and details for Google.
/// Anyone may call it. Cached for 10 minutes (cleared when staff or the AI change the text).
/// </summary>
/// <param name="Path">The website address, e.g. "/" or "/shops/{id}".</param>
[Cacheable(CacheTags.Seo, Minutes = 10)]
public sealed record GetPublicSeoQuery(string Path) : IRequest<Result<PublicSeoDto>>;

/// <summary>Handles <see cref="GetPublicSeoQuery"/>.</summary>
public sealed class GetPublicSeoQueryHandler(
    ISeoEntryRepository entries,
    IVendorRepository vendors,
    IFileStorage storage,
    ISiteAddresses site)
    : IRequestHandler<GetPublicSeoQuery, Result<PublicSeoDto>>
{
    /// <returns>The text (written, else Phloem's default); or <see cref="SeoErrors.PageNotFound"/> for any other
    /// address or a shop buyers can't see (the website then keeps its own default).</returns>
    public async Task<Result<PublicSeoDto>> Handle(GetPublicSeoQuery query, CancellationToken ct)
    {
        var path = "/" + query.Path.Trim().Trim('/');
        if (path.Length > 1) path = path.TrimEnd('/');

        // A fixed page
        if (SeoPages.FindMainByPath(path) is { } main)
        {
            var entry = await entries.GetByKeyAsync(main.Key, ct);
            return new PublicSeoDto(entry?.Title ?? main.FallbackTitle, entry?.Description ?? main.FallbackDescription,
                                    entry?.Keywords ?? string.Empty, null, $"{site.Website}{main.Path}", null);
        }

        // A shop
        if (SeoPages.ShopIdFromPath(path) is not { } shopId) return SeoErrors.PageNotFound;
        var shop = await vendors.GetByIdAsync(shopId, ct);
        if (shop is null || !shop.IsVisibleToBuyers) return SeoErrors.PageNotFound;

        var shopEntry = await entries.GetByKeyAsync(SeoPages.ShopKey(shop.Id), ct);
        var url = $"{site.Website}{SeoPages.ShopPath(shop.Id)}";
        var imagePath = shop.CoverPhoto?.Path ?? shop.Logo?.Path;
        var imageUrl = imagePath is null ? null : storage.GetPublicUrl(imagePath);
        var description = shopEntry?.Description ?? SeoPageLookup.ShopFallbackDescription(shop.Name, shop.Country);

        // Details for Google (schema.org): the shop, its country, its stars
        var jsonLd = JsonSerializer.Serialize(new Dictionary<string, object?>
        {
            ["@context"] = "https://schema.org",
            ["@type"] = "FoodEstablishment",
            ["name"] = shop.Name,
            ["url"] = url,
            ["description"] = description,
            ["image"] = imageUrl,
            ["servesCuisine"] = "African",
            ["address"] = new Dictionary<string, string>
            {
                ["@type"] = "PostalAddress",
                ["addressCountry"] = IsoCode(shop.Country)
            },
            ["aggregateRating"] = shop.RatingCount > 0
                ? new Dictionary<string, object>
                {
                    ["@type"] = "AggregateRating",
                    ["ratingValue"] = Math.Round(shop.Rating, 1),
                    ["ratingCount"] = shop.RatingCount
                }
                : null
        }.Where(p => p.Value is not null).ToDictionary(p => p.Key, p => p.Value));

        return new PublicSeoDto(shopEntry?.Title ?? SeoPageLookup.ShopFallbackTitle(shop.Name, shop.Country),
                                description, shopEntry?.Keywords ?? string.Empty, imageUrl, url, jsonLd);
    }

    private static string IsoCode(Country country) => country switch
    {
        Country.Ghana => "GH",
        Country.Nigeria => "NG",
        _ => "GB"
    };
}
