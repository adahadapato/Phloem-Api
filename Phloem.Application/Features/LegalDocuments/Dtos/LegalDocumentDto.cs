using Phloem.Domain.Entities;
using Phloem.Domain.Enums;

namespace Phloem.Application.Features.LegalDocuments.Dtos;

/// <summary>A legal text as the website and the app show it.</summary>
/// <param name="Type">Terms or Privacy.</param>
/// <param name="Version">1, 2, 3…</param>
/// <param name="Title">e.g. "Terms &amp; Conditions".</param>
/// <param name="Content">The text, in Markdown (headings may carry anchors like {#sellers}).</param>
/// <param name="IsDraft">True while the wording is temporary: show a "Draft" note.</param>
/// <param name="PublishedAt">When this version took effect (UTC).</param>
public sealed record LegalDocumentDto(
    LegalDocumentType Type,
    int Version,
    string Title,
    string Content,
    bool IsDraft,
    DateTime PublishedAt)
{
    /// <summary>Maps a version.</summary>
    /// <param name="d">The version.</param>
    /// <returns>The DTO.</returns>
    public static LegalDocumentDto From(LegalDocument d) =>
        new(d.Type, d.Version, d.Title, d.Content, d.IsDraft, d.PublishedAtUtc);
}
