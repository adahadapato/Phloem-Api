using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Phloem.Domain.Constants;
using Phloem.Domain.Entities;
using Phloem.Domain.Enums;
using Phloem.Domain.Interfaces;
using Phloem.Domain.Models;
using Phloem.Domain.Shared.Errors;
using Phloem.Domain.Shared.Results;

namespace Phloem.Application.Features.Seo.Common;

/// <summary>
/// Asks the AI ("Smart SEO" prompt) to write a page's search engine text and saves it. Used by "Write with AI" on
/// Admin → SEO and by the background writer (new shops, changed shops, pages never written).
/// </summary>
/// <remarks>
/// <para><b>What the AI gets:</b> only facts Phloem already shows on the page (shop name, country, ✓ badge, rating,
/// dishes with prices; or what a main page is for), as JSON, so text inside it can't act as instructions.</para>
/// <para><b>"Changed" pages:</b> each entry keeps a fingerprint of what the page looked like (name, country, badge,
/// number of dishes). When it no longer matches, an AI-written entry is rewritten; a staff-written one is left alone.</para>
/// </remarks>
public sealed class SeoWriter(
    IVendorRepository vendors,
    IDishRepository dishes,
    ISeoEntryRepository entries,
    IAiJsonClient ai,
    IUnitOfWork unitOfWork)
{
    /// <summary>What to send for one page.</summary>
    /// <param name="Key">The page key.</param>
    /// <param name="Kind">Fixed page or shop.</param>
    /// <param name="ShopId">The shop, for shop pages.</param>
    /// <param name="Name">The page's name (for the log).</param>
    /// <param name="Json">What the AI gets.</param>
    /// <param name="Fingerprint">What the page looks like now.</param>
    public sealed record PageInput(string Key, SeoPageKind Kind, Guid? ShopId, string Name, string Json, string Fingerprint);

    /// <summary>A country as people say it.</summary>
    public static string CountryName(Country country) => country switch
    {
        Country.UnitedKingdom => "United Kingdom",
        _ => country.ToString()
    };

    /// <summary>A shop's fingerprint: changes when its name, country, badge or number of dishes change.</summary>
    public static string ShopFingerprint(string name, Country country, bool verified, int dishCount) =>
        Fingerprint($"{name.Trim()}|{country}|{verified}|{dishCount}");

    /// <summary>A main page's fingerprint: changes when what the page is for changes.</summary>
    public static string MainFingerprint(SeoPageDefinition page) => Fingerprint(page.About);

    /// <summary>What to send for a fixed page.</summary>
    /// <param name="page">The page.</param>
    public static PageInput MainInput(SeoPageDefinition page) =>
        new(page.Key, SeoPageKind.Main, null, page.Name, JsonSerializer.Serialize(new
        {
            pageType = "Main page",
            name = page.Name,
            address = page.Path,
            countries = "United Kingdom, Ghana and Nigeria",
            about = page.About
        }), MainFingerprint(page));

    /// <summary>What to send for a shop, or null when the shop doesn't exist or buyers can't see it.</summary>
    /// <param name="shopId">The shop.</param>
    /// <param name="ct">Stops the queries.</param>
    public async Task<PageInput?> ShopInputAsync(Guid shopId, CancellationToken ct)
    {
        var shop = await vendors.GetByIdAsync(shopId, ct);
        if (shop is null || !shop.IsVisibleToBuyers) return null;

        var menu = (await dishes.GetByVendorAsync(shopId, ct)).OrderBy(d => d.Name).ToList();
        var json = JsonSerializer.Serialize(new
        {
            pageType = "Shop",
            name = shop.Name,
            address = SeoPages.ShopPath(shop.Id),
            country = CountryName(shop.Country),
            verified = shop.IsVerified,
            rating = shop.RatingCount > 0 ? Math.Round(shop.Rating, 1) : (decimal?)null,
            ratingCount = shop.RatingCount,
            dishes = menu.Take(40).Select(d => new
            {
                name = d.Name,
                category = d.Category.ToString(),
                price = d.Price,
                currency = d.Currency.ToString(),
                hasPhoto = d.Photos.Count > 0,
                hasDescription = !string.IsNullOrWhiteSpace(d.Description)
            })
        });
        return new PageInput(SeoPages.ShopKey(shop.Id), SeoPageKind.Shop, shop.Id, shop.Name, json,
                             ShopFingerprint(shop.Name, shop.Country, shop.IsVerified, menu.Count));
    }

    /// <summary>Asks the AI and saves its text (replacing whatever was there, staff wording included: only call this
    /// for staff entries when staff asked).</summary>
    /// <param name="input">The page.</param>
    /// <param name="requestedBy">The staff member who asked; null when it runs by itself.</param>
    /// <param name="ct">Stops the work.</param>
    /// <returns>The entry; or an AI failure (Disabled, SpendCapReached, ProviderFailed, BadAnswer).</returns>
    public async Task<Result<SeoEntry>> WriteAsync(PageInput input, Guid? requestedBy, CancellationToken ct)
    {
        var answer = await ai.AskAsync(new AiJsonRequest(AiPrompts.SmartSeo, input.Json, $"SEO: {input.Name}"), ct);
        if (answer.IsFailure) return answer.Error;

        var text = Read(answer.Value);
        if (text is null) return AiErrors.BadAnswer;

        var entry = await entries.GetByKeyAsync(input.Key, ct);
        if (entry is null)
        {
            entry = SeoEntry.WrittenByAi(input.Key, input.Kind, input.ShopId, text, input.Fingerprint,
                                         answer.Value.CallId, requestedBy);
            entries.Add(entry);
        }
        else
        {
            entry.ApplyAi(text, input.Fingerprint, answer.Value.CallId, requestedBy);
        }

        await unitOfWork.SaveChangesAsync(ct);
        return entry;
    }

    /// <summary>
    /// Writes up to <paramref name="max"/> pages that need it: fixed pages never written, live shops never written,
    /// and AI-written pages whose page has changed. Staff-written pages are skipped. Stops at the first AI failure
    /// (AI off, cap reached, service down), so nothing is wasted.
    /// </summary>
    /// <param name="max">Most pages to write in this run.</param>
    /// <param name="ct">Stops the work.</param>
    /// <returns>How many were written, and the AI failure that stopped the run (if any).</returns>
    public async Task<(int Written, Error? StoppedBy)> WriteMissingAsync(int max, CancellationToken ct)
    {
        var existing = await entries.GetAllAsync(ct);
        var todo = new List<Func<Task<PageInput?>>>();

        foreach (var page in SeoPages.Main)
        {
            if (NeedsWriting(existing.GetValueOrDefault(page.Key), MainFingerprint(page)))
                todo.Add(() => Task.FromResult<PageInput?>(MainInput(page)));
        }

        var shops = await vendors.GetVisibleForSeoAsync(ct);
        var counts = await dishes.CountByVendorsAsync(shops.Select(s => s.Id).ToList(), ct);
        foreach (var shop in shops)
        {
            var fingerprint = ShopFingerprint(shop.Name, shop.Country, shop.IsVerified, counts.GetValueOrDefault(shop.Id));
            if (NeedsWriting(existing.GetValueOrDefault(SeoPages.ShopKey(shop.Id)), fingerprint))
                todo.Add(() => ShopInputAsync(shop.Id, ct));
        }

        var written = 0;
        foreach (var next in todo.Take(Math.Max(0, max)))
        {
            var input = await next();
            if (input is null) continue;

            var result = await WriteAsync(input, null, ct);
            if (result.IsFailure) return (written, result.Error);
            written++;
        }
        return (written, null);
    }

    /// <summary>True when an entry is missing, or written by the AI for a page that has changed since.</summary>
    public static bool NeedsWriting(SeoEntry? entry, string fingerprint) =>
        entry is null || (entry.Source == SeoSource.Ai && entry.SourceFingerprint != fingerprint);

    /// <summary>The AI's answer as text for a page, or null when the title or description is missing.</summary>
    private static SeoSuggestion? Read(AiJsonAnswer answer)
    {
        var title = answer.Text("seoTitle")?.Trim();
        var description = answer.Text("metaDescription")?.Trim();
        if (string.IsNullOrWhiteSpace(title) || string.IsNullOrWhiteSpace(description)) return null;

        int? score = null;
        if (answer.Json.TryGetProperty("seoScore", out var s))
        {
            if (s.ValueKind == JsonValueKind.Number && s.TryGetDouble(out var d)) score = (int)Math.Round(d);
            else if (s.ValueKind == JsonValueKind.String && int.TryParse(s.GetString(), out var i)) score = i;
        }

        return new SeoSuggestion(title, description, List(answer.Json, "keywords"), List(answer.Json, "improvements"),
                                 score, answer.Text("scoreExplanation"));
    }

    /// <summary>A list field: an array of text, or one comma-separated text.</summary>
    private static IReadOnlyList<string> List(JsonElement json, string name)
    {
        if (!json.TryGetProperty(name, out var value)) return [];
        return value.ValueKind switch
        {
            JsonValueKind.Array => value.EnumerateArray()
                                        .Where(v => v.ValueKind == JsonValueKind.String)
                                        .Select(v => v.GetString()!.Trim())
                                        .Where(v => v.Length > 0).Take(10).ToList(),
            JsonValueKind.String => (value.GetString() ?? "")
                                    .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                                    .Take(10).ToList(),
            _ => []
        };
    }

    private static string Fingerprint(string text) =>
        Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(text)))[..16];
}
