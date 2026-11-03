namespace Phloem.Domain.Interfaces;

/// <summary>Phloem's public addresses (from <c>AppSettings</c>), for links in the sitemap and share pages.</summary>
public interface ISiteAddresses
{
    /// <summary>The website, e.g. "https://phloemmarket.com" (no "/" at the end).</summary>
    string Website { get; }

    /// <summary>The API, e.g. "https://phloemapi.phloemmarket.com" (no "/" at the end).</summary>
    string Api { get; }
}
