using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Phloem.Domain.Entities;

namespace Phloem.Persistence.Configurations;

/// <summary>
/// Table <c>LegalDocuments</c>: every version of the Terms and the Privacy Policy, never changed once published.
/// </summary>
/// <remarks>One row per (Type, Version) (unique index); the current text is the highest version of its type.
/// The content is long Markdown, stored as nvarchar(max).</remarks>
public sealed class LegalDocumentConfiguration : EntityConfiguration<LegalDocument>
{
    /// <inheritdoc />
    protected override void ConfigureEntity(EntityTypeBuilder<LegalDocument> builder)
    {
        builder.ToTable("LegalDocuments");

        builder.Property(d => d.Type).IsRequired();
        builder.Property(d => d.Version).IsRequired();
        builder.HasIndex(d => new { d.Type, d.Version }).IsUnique();

        builder.Property(d => d.Title).IsRequired().HasMaxLength(LegalDocument.TitleMaxLength);
        builder.Property(d => d.Content).IsRequired();   // nvarchar(max); the Domain caps the length
        builder.Property(d => d.IsDraft).IsRequired();
        builder.Property(d => d.PublishedAtUtc).IsRequired();
    }
}
