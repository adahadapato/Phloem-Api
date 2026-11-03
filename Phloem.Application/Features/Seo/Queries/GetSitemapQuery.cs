using MediatR;
using Phloem.Application.Common.Caching;
using Phloem.Application.Features.Seo.Common;
using Phloem.Application.Features.Seo.Dtos;
using Phloem.Domain.Constants;
using Phloem.Domain.Interfaces;
using Phloem.Domain.Shared.Results;

namespace Phloem.Application.Features.Seo.Queries;

/// <summary>The sitemap: every public address search engines should know (fixed pages, live shops, Terms, Privacy).
/// Anyone may call it. Cached for 60 minutes.</summary>
[Cacheable(CacheTags.Seo, Minutes = 60)]
public sealed record GetSitemapQuery : IRequest<Result<IReadOnlyList<SitemapUrlDto>>>;

/// <summary>Handles <see cref="GetSitemapQuery"/>.</summary>
public sealed class GetSitemapQueryHandler(IVendorRepository vendors)
    : IRequestHandler<GetSitemapQuery, Result<IReadOnlyList<SitemapUrlDto>>>
{
    /// <summary>Fixed pages first, then each live shop with the day it last changed.</summary>
    public async Task<Result<IReadOnlyList<SitemapUrlDto>>> Handle(GetSitemapQuery query, CancellationToken ct)
    {
        var urls = SeoPages.Main.Select(p => new SitemapUrlDto(p.Path, null))
                           .Concat(SitemapExtras.Paths.Select(p => new SitemapUrlDto(p, null)))
                           .ToList();
        urls.AddRange((await vendors.GetVisibleForSeoAsync(ct))
                      .Select(s => new SitemapUrlDto(SeoPages.ShopPath(s.Id), s.ChangedAtUtc)));
        return urls;
    }
}
