using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Phloem.Domain.Entities;
using Phloem.Domain.ValueObjects;

namespace Phloem.Persistence.Configurations;

/// <summary>
/// Table <c>VendorDocuments</c>: documents a shop uploaded for review, with the file's details as columns.
/// Child of <see cref="Vendor"/>; the relationship is configured in <see cref="VendorConfiguration"/>.
/// </summary>
/// <remarks>
/// <para>One document per type per shop (unique index). A new upload replaces the old row; EF Core deletes the old
/// one before inserting the new one in the same save, so the index never clashes.</para>
/// <para><c>ReviewedByUserId</c> is the admin's Identity Id, stored without a foreign key: it's a record of who
/// reviewed, and must stay even if that admin account is later removed.</para>
/// </remarks>
public sealed class VendorDocumentConfiguration : EntityConfiguration<VendorDocument>
{
    /// <inheritdoc />
    protected override void ConfigureEntity(EntityTypeBuilder<VendorDocument> builder)
    {
        builder.ToTable("VendorDocuments");

        builder.Property(d => d.VendorId).IsRequired();
        builder.Property(d => d.Type).IsRequired();
        builder.Property(d => d.Status).IsRequired();
        builder.Property(d => d.RejectionReason).HasMaxLength(Vendor.RejectionReasonMaxLength);
        builder.Property(d => d.ReviewedByUserId);
        builder.Property(d => d.ReviewedAt);
        builder.Property(d => d.ExpiresAt);

        builder.OwnsOne(d => d.File, file =>
        {
            file.Property(f => f.Path).IsRequired().HasMaxLength(DocumentFile.PathMaxLength);
            file.Property(f => f.OriginalFileName).IsRequired().HasMaxLength(DocumentFile.OriginalFileNameMaxLength);
            file.Property(f => f.ContentType).IsRequired().HasMaxLength(DocumentFile.ContentTypeMaxLength);
            file.Property(f => f.FileSizeBytes).IsRequired();
            file.Property(f => f.Hash).IsRequired().HasMaxLength(DocumentFile.HashMaxLength);
        });
        builder.Navigation(d => d.File).IsRequired();

        builder.HasIndex(d => new { d.VendorId, d.Type }).IsUnique();   // one per type per shop
        builder.HasIndex(d => d.ExpiresAt);                               // the document-expiry job
    }
}
