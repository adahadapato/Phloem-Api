using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Phloem.Domain.Entities;
using Phloem.Domain.ValueObjects;
using Phloem.Persistence.Converters;
using Phloem.Persistence.Mappings;

namespace Phloem.Persistence.Configurations;

/// <summary>
/// Table <c>Vendors</c>: a seller's shop, with its logo and cover photo stored as columns, and its documents in
/// <c>VendorDocuments</c> (configured here, through the private <c>_documents</c> field).
/// </summary>
/// <remarks>
/// <para><b>Unique:</b> one shop per owner (<c>OwnerId</c>) and shop names (<c>Name</c>). On SQL Server's default
/// collation, name comparison is case-insensitive ("Mama's Kitchen" = "MAMA'S KITCHEN").</para>
/// <para><b>Closed shops</b> are soft-deleted, so the base filter hides them; their rows stay because old orders point
/// at them. The unique indexes still include them, so a closed shop's name stays taken.</para>
/// </remarks>
public sealed class VendorConfiguration : EntityConfiguration<Vendor>
{
    /// <inheritdoc />
    protected override void ConfigureEntity(EntityTypeBuilder<Vendor> builder)
    {
        builder.ToTable("Vendors");

        // ---------- Owner ----------
        builder.HasOne(v => v.Owner)
               .WithMany()
               .HasForeignKey(v => v.OwnerId)
               .IsRequired()
               .OnDelete(DeleteBehavior.Restrict);      // profiles are soft-deleted, never removed
        builder.HasIndex(v => v.OwnerId).IsUnique();    // one shop per user

        // ---------- Business details ----------
        builder.Property(v => v.Name).IsRequired().HasMaxLength(Vendor.NameMaxLength);
        builder.HasIndex(v => v.Name).IsUnique();

        builder.Property(v => v.BusinessEmail)
               .IsRequired()
               .HasConversion(new EmailAddressConverter())
               .HasMaxLength(EmailAddress.MaxLength);

        // Optional: converter and size come from PhloemDbContext.ConfigureConventions
        // (an explicit converter here would trip the nullable warning, since it's typed for PhoneNumber, not PhoneNumber?)
        builder.Property(v => v.BusinessPhone)
               .HasMaxLength(PhoneNumberConverter.MaxLength);

        builder.Property(v => v.Country).IsRequired();
        builder.Property(v => v.DeliveryFee).HasMoneyPrecision();    // null = country minimum

        // ---------- Images (columns in this table) ----------
        builder.OwnsOne(v => v.Logo, logo => logo.MapImageFile());
        builder.OwnsOne(v => v.CoverPhoto, cover => cover.MapImageFile());

        // ---------- Status ----------
        builder.Property(v => v.Status).IsRequired();
        builder.Property(v => v.IsVerified).IsRequired();
        builder.Property(v => v.IsOwnerSuspended).IsRequired();   // hidden while the owner is suspended
        builder.Property(v => v.RejectionReason).HasMaxLength(Vendor.RejectionReasonMaxLength);
        builder.Property(v => v.ApprovedAtUtc);

        // ---------- Reputation & money ----------
        builder.Property(v => v.Rating).IsRequired().HasPrecision(3, 2);   // 0.00–5.00
        builder.Property(v => v.RatingCount).IsRequired();
        builder.Property(v => v.Balance).IsRequired().HasMoneyPrecision();

        // ---------- Documents ----------
        builder.HasMany(v => v.Documents)
               .WithOne()
               .HasForeignKey(d => d.VendorId)
               .IsRequired()
               .OnDelete(DeleteBehavior.Cascade);
        builder.Navigation(v => v.Documents)
               .HasField("_documents")
               .UsePropertyAccessMode(PropertyAccessMode.Field);

        // ---------- Calculated, not columns ----------
        builder.Ignore(v => v.Currency);
        builder.Ignore(v => v.IsVisibleToBuyers);

        // ---------- Lists ----------
        builder.HasIndex(v => new { v.Country, v.Status });   // buyers' shop list, admin filters
    }
}