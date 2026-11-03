using MediatR;
using Phloem.Application.Common.Caching;
using Phloem.Domain.Shared.Results;

namespace Phloem.Application.Features.Seo.Commands;

/// <summary>Admin → SEO → "Write with AI": the AI writes this page's text now (replacing staff wording too, because
/// staff asked). Used at once.</summary>
/// <param name="Key">"home", "shop-…".</param>
[InvalidatesCache(CacheTags.Seo)]
public sealed record WriteSeoPageCommand(string Key) : IRequest<Result>;

/// <summary>Admin → SEO → "Save": staff's own title, description and keywords. The AI won't change it by itself.</summary>
/// <param name="Key">"home", "shop-…".</param>
/// <param name="Title">The title Google shows.</param>
/// <param name="Description">The description Google shows.</param>
/// <param name="Keywords">Search phrases.</param>
[InvalidatesCache(CacheTags.Seo)]
public sealed record SaveSeoPageCommand(string Key, string Title, string Description, IReadOnlyList<string>? Keywords)
    : IRequest<Result>;

/// <summary>Writes the pages that need it (never written, or changed since the AI wrote them; staff wording is
/// left alone). Run by itself every 30 minutes, or from Admin → SEO → "Write missing pages now".</summary>
/// <param name="Max">Most pages in one go.</param>
/// <returns>How many were written.</returns>
[InvalidatesCache(CacheTags.Seo)]
public sealed record WriteMissingSeoCommand(int Max = 25) : IRequest<Result<int>>;
