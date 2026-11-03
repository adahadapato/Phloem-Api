namespace Phloem.Domain.Models;

/// <summary>One fixed page of the website that has search engine text (see <see cref="Constants.SeoPages"/>).</summary>
/// <param name="Key">Stable id, e.g. "home".</param>
/// <param name="Name">What staff see, e.g. "Home".</param>
/// <param name="Path">The website address, e.g. "/explore".</param>
/// <param name="About">What the page is for (sent to the AI, shown to staff).</param>
/// <param name="FallbackTitle">Used until someone (or the AI) writes the text.</param>
/// <param name="FallbackDescription">Used until someone (or the AI) writes the text.</param>
public sealed record SeoPageDefinition(
    string Key,
    string Name,
    string Path,
    string About,
    string FallbackTitle,
    string FallbackDescription);
