using MediatR;
using Phloem.Application.Features.Seo.Common;
using Phloem.Application.Features.Seo.Dtos;
using Phloem.Domain.Enums;
using Phloem.Domain.Interfaces;
using Phloem.Domain.Shared.Results;

namespace Phloem.Application.Features.Seo.Queries;

/// <summary>Admin → SEO, "Website health": counts, sitemap and robots links. Not cached.</summary>
public sealed record GetSeoOverviewQuery : IRequest<Result<SeoOverviewDto>>;

/// <summary>Handles <see cref="GetSeoOverviewQuery"/>.</summary>
public sealed class GetSeoOverviewQueryHandler(SeoPageLookup lookup, IAiSettingsProvider ai, ISiteAddresses site)
    : IRequestHandler<GetSeoOverviewQuery, Result<SeoOverviewDto>>
{
    /// <summary>Counts every page by status.</summary>
    public async Task<Result<SeoOverviewDto>> Handle(GetSeoOverviewQuery query, CancellationToken ct)
    {
        var pages = await lookup.AllAsync(ct);
        var main = pages.Where(p => p.Row.Kind == SeoPageKind.Main).ToList();
        var shops = pages.Where(p => p.Row.Kind == SeoPageKind.Shop).ToList();
        var settings = ai.Get();

        return new SeoOverviewDto(
            settings.IsEnabled, settings.NotReadyReason,
            main.Count, main.Count(p => p.Entry is not null),
            shops.Count, shops.Count(p => p.Entry is not null),
            shops.Count(p => p.Row.Status == SeoPageStatus.EditedByStaff),
            pages.Count(p => p.Row.PageChanged),
            $"{site.Api}/sitemap.xml", $"{site.Website}/robots.txt", pages.Count + SitemapExtras.Count);
    }
}
