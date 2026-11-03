using Phloem.Application.Features.Seo.Dtos;
using Phloem.Domain.Constants;
using Phloem.Domain.Entities;
using Phloem.Domain.Enums;
using Phloem.Domain.Interfaces;
using Phloem.Domain.Models;

namespace Phloem.Application.Features.Seo.Common;

/// <summary>Every page with search engine text (fixed pages + live shops) and its entry, for Admin → SEO.</summary>
public sealed class SeoPageLookup(IVendorRepository vendors, IDishRepository dishes, ISeoEntryRepository entries)
{
    /// <summary>One page, ready for the list.</summary>
    /// <param name="Row">The list row.</param>
    /// <param name="Entry">Its entry, if written.</param>
    /// <param name="Main">The fixed page (fixed pages only).</param>
    /// <param name="Shop">The shop (shops only).</param>
    public sealed record Page(SeoPageRowDto Row, SeoEntry? Entry, SeoPageDefinition? Main, SeoShop? Shop);

    /// <summary>Every page (fixed pages first, then shops by name).</summary>
    /// <param name="ct">Stops the queries.</param>
    public async Task<IReadOnlyList<Page>> AllAsync(CancellationToken ct)
    {
        var existing = await entries.GetAllAsync(ct);
        var pages = new List<Page>();

        foreach (var main in SeoPages.Main)
        {
            var entry = existing.GetValueOrDefault(main.Key);
            pages.Add(new Page(Row(main.Key, SeoPageKind.Main, main.Name, main.Path, null, entry,
                                   SeoWriter.MainFingerprint(main), main.FallbackTitle), entry, main, null));
        }

        var shops = await vendors.GetVisibleForSeoAsync(ct);
        var counts = await dishes.CountByVendorsAsync(shops.Select(s => s.Id).ToList(), ct);
        foreach (var shop in shops)
        {
            var key = SeoPages.ShopKey(shop.Id);
            var entry = existing.GetValueOrDefault(key);
            var fingerprint = SeoWriter.ShopFingerprint(shop.Name, shop.Country, shop.IsVerified,
                                                        counts.GetValueOrDefault(shop.Id));
            pages.Add(new Page(Row(key, SeoPageKind.Shop, shop.Name, SeoPages.ShopPath(shop.Id), shop.Country, entry,
                                   fingerprint, ShopFallbackTitle(shop.Name, shop.Country)), entry, null, shop));
        }
        return pages;
    }

    /// <summary>Phloem's simple title for a shop with no written text.</summary>
    public static string ShopFallbackTitle(string name, Country country) =>
        $"{name}: home-cooked food in {SeoWriter.CountryName(country)} | Phloem";

    /// <summary>Phloem's simple description for a shop with no written text.</summary>
    public static string ShopFallbackDescription(string name, Country country) =>
        $"Order home-cooked African food from {name} in {SeoWriter.CountryName(country)} on Phloem. " +
        "See the dishes, prices and ratings, and order for delivery.";

    private static SeoPageRowDto Row(string key, SeoPageKind kind, string name, string path, Country? country,
                                     SeoEntry? entry, string fingerprint, string fallbackTitle) =>
        new(key, kind, name, path, country, StatusOf(entry),
            entry is { Source: SeoSource.Ai } && entry.SourceFingerprint != fingerprint,
            entry?.Title ?? fallbackTitle, entry?.Score);

    /// <summary>Not written / by AI / by staff.</summary>
    public static SeoPageStatus StatusOf(SeoEntry? entry) => entry switch
    {
        null => SeoPageStatus.NotWritten,
        { Source: SeoSource.Staff } => SeoPageStatus.EditedByStaff,
        _ => SeoPageStatus.WrittenByAi
    };
}
