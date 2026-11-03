namespace Phloem.Domain.Models;

/// <summary>Search engine text for one page, as the AI (or staff) wrote it.</summary>
/// <param name="Title">The title Google shows (blue link).</param>
/// <param name="Description">The two lines under it.</param>
/// <param name="Keywords">Search phrases the page should be found for.</param>
/// <param name="Improvements">Tips to make the page better (AI only).</param>
/// <param name="Score">How ready the page is, 0–100 (AI's editorial estimate, not a Google score).</param>
/// <param name="ScoreExplanation">Why that score.</param>
public sealed record SeoSuggestion(
    string Title,
    string Description,
    IReadOnlyList<string> Keywords,
    IReadOnlyList<string> Improvements,
    int? Score,
    string? ScoreExplanation);
