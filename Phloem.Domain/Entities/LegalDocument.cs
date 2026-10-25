using Phloem.Domain.Enums;

namespace Phloem.Domain.Entities;

/// <summary>
/// One VERSION of a legal text (Terms, Privacy). Versions are never changed or deleted once published: a new
/// wording is a new row with the next version number, so the exact text anyone agreed to can always be shown later.
/// The current text of a type is its highest version.
/// </summary>
/// <remarks>
/// The content is plain Markdown (headings, lists, bold, links), so the website and the app can each show it
/// in their own style. Headings may carry fixed anchors (e.g. <c>{#sellers}</c>) so links can open a section.
/// Bad values are checked by the validator first, so they THROW here.
/// </remarks>
public class LegalDocument : Entity<Guid>
{
    /// <summary>Longest title.</summary>
    public const int TitleMaxLength = 150;

    /// <summary>Longest text (about 100 pages; a guard, not a target).</summary>
    public const int ContentMaxLength = 200_000;

    /// <summary>Required by EF Core.</summary>
    protected LegalDocument() { }

    private LegalDocument(Guid id, LegalDocumentType type, int version, string title, string content, bool isDraft,
                          DateTime publishedAtUtc)
        : base(id)
    {
        Type = type;
        Version = version;
        Title = title.Trim();
        Content = content.Trim();
        IsDraft = isDraft;
        PublishedAtUtc = publishedAtUtc;
    }

    /// <summary>Which text this is.</summary>
    public LegalDocumentType Type { get; private set; }

    /// <summary>1, 2, 3… per type (unique with <see cref="Type"/>).</summary>
    public int Version { get; private set; }

    /// <summary>e.g. "Terms &amp; Conditions".</summary>
    public string Title { get; private set; } = string.Empty;

    /// <summary>The text, in Markdown.</summary>
    public string Content { get; private set; } = string.Empty;

    /// <summary>True for a temporary wording not yet reviewed by a lawyer (the website and app say so).</summary>
    public bool IsDraft { get; private set; }

    /// <summary>When this version took effect (UTC).</summary>
    public DateTime PublishedAtUtc { get; private set; }

    /// <summary>Publishes a version (admin, or the starter data).</summary>
    /// <param name="type">Terms or Privacy.</param>
    /// <param name="version">The next number for this type (1 for the first).</param>
    /// <param name="title">Not empty, at most <see cref="TitleMaxLength"/> characters.</param>
    /// <param name="content">Markdown, not empty, at most <see cref="ContentMaxLength"/> characters.</param>
    /// <param name="isDraft">True while the wording is temporary.</param>
    /// <param name="publishedAtUtc">When it takes effect (UTC).</param>
    /// <returns>The new version.</returns>
    /// <exception cref="ArgumentException">Empty title or content, or a version below 1.</exception>
    /// <exception cref="ArgumentOutOfRangeException">Title or content too long, or an unknown type.</exception>
    public static LegalDocument Publish(LegalDocumentType type, int version, string title, string content, bool isDraft,
                                        DateTime publishedAtUtc)
    {
        if (!Enum.IsDefined(type)) throw new ArgumentOutOfRangeException(nameof(type));
        ArgumentOutOfRangeException.ThrowIfLessThan(version, 1);
        ArgumentException.ThrowIfNullOrWhiteSpace(title);
        ArgumentException.ThrowIfNullOrWhiteSpace(content);
        ArgumentOutOfRangeException.ThrowIfGreaterThan(title.Trim().Length, TitleMaxLength, nameof(title));
        ArgumentOutOfRangeException.ThrowIfGreaterThan(content.Trim().Length, ContentMaxLength, nameof(content));

        return new LegalDocument(Guid.CreateVersion7(), type, version, title, content, isDraft, publishedAtUtc);
    }
}
