namespace Phloem.Domain.Shared.Errors;

/// <summary>Every business-rule failure for search engine text (SEO). Nothing here is thrown.</summary>
public static class SeoErrors
{
    /// <summary>No such page, or a shop buyers can't see (not approved, blocked).</summary>
    public static readonly Error PageNotFound = Error.NotFound(
        "Seo.PageNotFound", "There's no public page with that name.");
}
