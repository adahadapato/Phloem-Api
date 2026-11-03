namespace Phloem.Application.Features.Seo.Common;

/// <summary>Public pages in the sitemap that have no search engine text of their own (legal pages).</summary>
public static class SitemapExtras
{
    /// <summary>The addresses.</summary>
    public static IReadOnlyList<string> Paths { get; } = ["/terms", "/privacy"];

    /// <summary>How many.</summary>
    public static int Count => Paths.Count;
}
