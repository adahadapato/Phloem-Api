using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Phloem.Domain.Entities;

namespace Phloem.Persistence.Configurations;

/// <summary>Table <c>TermsAcceptances</c>: proof of who accepted which Terms / was shown which Privacy Policy.</summary>
/// <remarks>Read as "latest version per person and document", hence the index. Rows are never changed or deleted.
/// <c>UserId</c> isn't a foreign key, so the proof survives an account deletion.</remarks>
public sealed class TermsAcceptanceConfiguration : EntityConfiguration<TermsAcceptance>
{
    /// <inheritdoc />
    protected override void ConfigureEntity(EntityTypeBuilder<TermsAcceptance> builder)
    {
        builder.ToTable("TermsAcceptances");

        builder.Property(a => a.UserId).IsRequired();
        builder.Property(a => a.DocumentType).IsRequired();
        builder.Property(a => a.Version).IsRequired();
        builder.Property(a => a.Context).IsRequired();
        builder.Property(a => a.Reference).HasMaxLength(TermsAcceptance.ReferenceMaxLength);

        builder.HasIndex(a => new { a.UserId, a.DocumentType, a.Version });
        builder.HasIndex(a => new { a.DocumentType, a.Version });
    }
}
