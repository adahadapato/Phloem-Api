using MediatR;
using Phloem.Application.Features.Seo.Common;
using Phloem.Application.Features.Seo.Dtos;
using Phloem.Domain.Enums;
using Phloem.Domain.Shared.Results;

namespace Phloem.Application.Features.Seo.Queries;

/// <summary>Admin → SEO: the pages, with filters. Not cached.</summary>
/// <param name="Kind">Fixed pages or shops; null = both.</param>
/// <param name="Status">Only pages in this state; null = all.</param>
/// <param name="ChangedOnly">Only AI-written pages whose page changed since (optional; missing = false).</param>
/// <param name="Search">Part of the page or shop name.</param>
/// <param name="Country">Only shops in this country.</param>
/// <param name="Page">1-based page.</param>
/// <param name="PageSize">Rows per page.</param>
public sealed record GetSeoPagesQuery(SeoPageKind? Kind, SeoPageStatus? Status, bool? ChangedOnly, string? Search,
                                      Country? Country, int Page = 1, int PageSize = 20)
    : IRequest<Result<PagedResult<SeoPageRowDto>>>;

/// <summary>Handles <see cref="GetSeoPagesQuery"/>.</summary>
public sealed class GetSeoPagesQueryHandler(SeoPageLookup lookup)
    : IRequestHandler<GetSeoPagesQuery, Result<PagedResult<SeoPageRowDto>>>
{
    /// <summary>Filters the full page list (one row per fixed page and live shop) and returns one page of it.</summary>
    public async Task<Result<PagedResult<SeoPageRowDto>>> Handle(GetSeoPagesQuery query, CancellationToken ct)
    {
        var page = new PageRequest { Page = query.Page, PageSize = query.PageSize };
        var rows = (await lookup.AllAsync(ct)).Select(p => p.Row);

        if (query.Kind is { } kind) rows = rows.Where(r => r.Kind == kind);
        if (query.Status is { } status) rows = rows.Where(r => r.Status == status);
        if (query.ChangedOnly == true) rows = rows.Where(r => r.PageChanged);
        if (query.Country is { } country) rows = rows.Where(r => r.Country == country);
        if (!string.IsNullOrWhiteSpace(query.Search))
        {
            var term = query.Search.Trim();
            rows = rows.Where(r => r.Name.Contains(term, StringComparison.OrdinalIgnoreCase));
        }

        var all = rows.ToList();
        return new PagedResult<SeoPageRowDto>(all.Skip(page.Skip).Take(page.PageSize).ToList(), all.Count, page.Page,
                                              page.PageSize);
    }
}
