using MediatR;
using Phloem.Application.Features.Seo.Common;
using Phloem.Application.Features.Seo.Dtos;
using Phloem.Domain.Constants;
using Phloem.Domain.Enums;
using Phloem.Domain.Interfaces;
using Phloem.Domain.Shared.Errors;
using Phloem.Domain.Shared.Results;

namespace Phloem.Application.Features.Seo.Queries;

/// <summary>Admin → SEO → one page: its text, Google preview, the AI's tips and score. Not cached.</summary>
/// <param name="Key">"home", "shop-…".</param>
public sealed record GetSeoPageQuery(string Key) : IRequest<Result<SeoPageDetailsDto>>;

/// <summary>Handles <see cref="GetSeoPageQuery"/>.</summary>
public sealed class GetSeoPageQueryHandler(
    SeoPageLookup lookup,
    IUserProfileRepository profiles,
    ISiteAddresses site)
    : IRequestHandler<GetSeoPageQuery, Result<SeoPageDetailsDto>>
{
    /// <returns>The page; or <see cref="SeoErrors.PageNotFound"/> (unknown page, or a shop buyers can't see).</returns>
    public async Task<Result<SeoPageDetailsDto>> Handle(GetSeoPageQuery query, CancellationToken ct)
    {
        var key = query.Key.Trim();
        var p = (await lookup.AllAsync(ct)).FirstOrDefault(x => x.Row.Key == key);
        if (p is null) return SeoErrors.PageNotFound;

        var e = p.Entry;
        string? changedBy = null;
        if (e?.ChangedBy is { } by)
            changedBy = (await profiles.GetFullNamesAsync([by], ct)).GetValueOrDefault(by, "Unknown user");
        else if (e is { Source: SeoSource.Ai }) changedBy = "Written by itself";

        var description = e?.Description
                          ?? p.Main?.FallbackDescription
                          ?? SeoPageLookup.ShopFallbackDescription(p.Row.Name, p.Row.Country ?? Country.UnitedKingdom);

        var whatTheAiGets = p.Main is { } main
            ? $"That this is Phloem's \"{main.Name}\" page ({main.Path}) and what it's for: {main.About}"
            : "The shop's name, country, ✓ badge, rating, and its dishes (name, category, price, whether each has a " +
              "photo and a description). Nothing private.";

        return new SeoPageDetailsDto(
            p.Row.Key, p.Row.Kind, p.Row.Name, p.Row.Path, $"{site.Website}{p.Row.Path}",
            p.Shop is { } shop ? $"{site.Api}/share/shops/{shop.Id:D}" : null,
            p.Row.Country, p.Row.Status, p.Row.PageChanged, p.Row.Title, description,
            e?.KeywordList() ?? [], e?.ImprovementList() ?? [], e?.Score, e?.ScoreExplanation, whatTheAiGets,
            changedBy, e is null ? null : e.UpdatedAt ?? e.CreatedAt);
    }
}
