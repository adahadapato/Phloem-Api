using Phloem.Domain.Constants;
using Phloem.Domain.Enums;
using Phloem.Domain.Models;

namespace Phloem.Domain.Entities;

/// <summary>
/// The search engine text (SEO) for one website page: what Google shows for it, plus the AI's keywords, tips and
/// score for staff.
/// </summary>
/// <remarks>
/// <para><b>Who writes it:</b> the AI, by itself, for new shops and when a shop changes
/// (<see cref="SourceFingerprint"/> no longer matches); or staff on Admin → SEO. Staff wording is never overwritten
/// automatically.</para>
/// <para>One row per page (<see cref="Key"/>: "home", "shop-{id}"…).</para>
/// </remarks>
public class SeoEntry : Entity<Guid>
{
    /// <summary>Longest key.</summary>
    public const int KeyMaxLength = 80;

    /// <summary>Longest keywords text (comma-separated).</summary>
    public const int KeywordsMaxLength = 500;

    /// <summary>Longest tips text (one per line).</summary>
    public const int ImprovementsMaxLength = 2000;

    /// <summary>Longest score explanation.</summary>
    public const int ScoreExplanationMaxLength = 500;

    /// <summary>Longest fingerprint.</summary>
    public const int FingerprintMaxLength = 64;

    /// <summary>For EF Core.</summary>
    protected SeoEntry() { }

    private SeoEntry(Guid id) : base(id) { }

    /// <summary>The page, e.g. "home" or "shop-…".</summary>
    public string Key { get; private set; } = string.Empty;

    /// <summary>Fixed page or shop.</summary>
    public SeoPageKind Kind { get; private set; }

    /// <summary>The shop, for shop pages.</summary>
    public Guid? ShopId { get; private set; }

    /// <summary>The title Google shows.</summary>
    public string Title { get; private set; } = string.Empty;

    /// <summary>The description Google shows.</summary>
    public string Description { get; private set; } = string.Empty;

    /// <summary>Search phrases, comma-separated.</summary>
    public string Keywords { get; private set; } = string.Empty;

    /// <summary>The AI's tips, one per line.</summary>
    public string Improvements { get; private set; } = string.Empty;

    /// <summary>The AI's score, 0–100.</summary>
    public int? Score { get; private set; }

    /// <summary>Why that score.</summary>
    public string? ScoreExplanation { get; private set; }

    /// <summary>Who wrote the current text.</summary>
    public SeoSource Source { get; private set; }

    /// <summary>A short code for what the page looked like when the AI wrote the text; a different code means the
    /// page has changed since.</summary>
    public string? SourceFingerprint { get; private set; }

    /// <summary>The AI call that wrote it (in the AI log).</summary>
    public Guid? AiCallId { get; private set; }

    /// <summary>The staff member who last wrote or asked for it.</summary>
    public Guid? ChangedBy { get; private set; }

    /// <summary>A new entry written by the AI.</summary>
    /// <param name="key">The page.</param>
    /// <param name="kind">Fixed page or shop.</param>
    /// <param name="shopId">The shop, for shop pages.</param>
    /// <param name="text">What the AI wrote.</param>
    /// <param name="fingerprint">What the page looked like.</param>
    /// <param name="aiCallId">The AI call.</param>
    /// <param name="requestedBy">The staff member who asked (null when it ran by itself).</param>
    public static SeoEntry WrittenByAi(string key, SeoPageKind kind, Guid? shopId, SeoSuggestion text,
                                       string fingerprint, Guid aiCallId, Guid? requestedBy)
    {
        if (string.IsNullOrWhiteSpace(key) || key.Length > KeyMaxLength)
            throw new ArgumentException("A page key is required.", nameof(key));

        var entry = new SeoEntry(Guid.CreateVersion7()) { Key = key, Kind = kind, ShopId = shopId };
        entry.ApplyAi(text, fingerprint, aiCallId, requestedBy);
        return entry;
    }

    /// <summary>Replaces the text with the AI's.</summary>
    public void ApplyAi(SeoSuggestion text, string fingerprint, Guid aiCallId, Guid? requestedBy)
    {
        Set(text);
        Improvements = Cut(string.Join('\n', text.Improvements.Where(t => !string.IsNullOrWhiteSpace(t))
                                                              .Select(t => t.Trim())), ImprovementsMaxLength);
        Score = text.Score is { } s ? Math.Clamp(s, 0, 100) : null;
        ScoreExplanation = string.IsNullOrWhiteSpace(text.ScoreExplanation)
            ? null : Cut(text.ScoreExplanation.Trim(), ScoreExplanationMaxLength);
        Source = SeoSource.Ai;
        SourceFingerprint = Cut(fingerprint, FingerprintMaxLength);
        AiCallId = aiCallId;
        ChangedBy = requestedBy;
    }

    /// <summary>Staff's own wording (title, description, keywords). Tips and score stay as the AI left them.</summary>
    /// <param name="title">The title.</param>
    /// <param name="description">The description.</param>
    /// <param name="keywords">The keywords.</param>
    /// <param name="staffId">Who.</param>
    public void EditByStaff(string title, string description, IReadOnlyList<string> keywords, Guid staffId)
    {
        Set(new SeoSuggestion(title, description, keywords, [], null, null));
        Source = SeoSource.Staff;
        ChangedBy = staffId;
    }

    /// <summary>A new entry written by staff (no AI text yet).</summary>
    public static SeoEntry WrittenByStaff(string key, SeoPageKind kind, Guid? shopId, string title,
                                          string description, IReadOnlyList<string> keywords, Guid staffId)
    {
        var entry = new SeoEntry(Guid.CreateVersion7()) { Key = key, Kind = kind, ShopId = shopId };
        entry.EditByStaff(title, description, keywords, staffId);
        return entry;
    }

    /// <summary>The keywords as a list.</summary>
    public IReadOnlyList<string> KeywordList() =>
        Keywords.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

    /// <summary>The tips as a list.</summary>
    public IReadOnlyList<string> ImprovementList() =>
        Improvements.Split('\n', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

    private void Set(SeoSuggestion text)
    {
        if (string.IsNullOrWhiteSpace(text.Title)) throw new ArgumentException("A title is required.", nameof(text));
        if (string.IsNullOrWhiteSpace(text.Description))
            throw new ArgumentException("A description is required.", nameof(text));

        Title = Cut(text.Title.Trim(), SeoPages.TitleMaxLength);
        Description = Cut(text.Description.Trim(), SeoPages.DescriptionMaxLength);
        Keywords = Cut(string.Join(", ", text.Keywords.Select(k => k.Replace(',', ' ').Trim())
                                                      .Where(k => k.Length > 0).Distinct()), KeywordsMaxLength);
    }

    private static string Cut(string value, int max) => value.Length <= max ? value : value[..max];
}
