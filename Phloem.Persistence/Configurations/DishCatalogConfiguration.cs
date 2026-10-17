using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Phloem.Domain.Entities;
using Phloem.Domain.Enums;

namespace Phloem.Persistence.Configurations;

/// <summary>
/// Table <c>DishCatalogs</c>: the approved list of dish names every seller picks from (plus sellers' suggestions
/// waiting for review), with their price ranges per country in <c>DishPriceRanges</c> (configured here, through the
/// private <c>_priceRanges</c> field).
/// </summary>
/// <remarks>
/// <para><b>Unique names:</b> SQL Server's default collation compares case-insensitively, so "jollof rice" and
/// "Jollof Rice" can't both exist, matching the Domain's rule.</para>
/// <para><b>Rejected suggestions</b> stay in the table (Status = Rejected) as a record, but don't hold their name:
/// the unique index is filtered to skip them, so the same name can be suggested again. This matches
/// <c>IDishCatalogRepository.NameExistsAsync</c>, where only Approved and Suggested names count.</para>
/// <para><c>SuggestedByUserId</c> is the seller's Id (same as their UserProfile), stored without a foreign key:
/// it's a record of who asked, and starter-list entries have none.</para>
/// </remarks>
public sealed class DishCatalogConfiguration : EntityConfiguration<DishCatalog>
{
    /// <inheritdoc />
    protected override void ConfigureEntity(EntityTypeBuilder<DishCatalog> builder)
    {
        builder.ToTable("DishCatalogs");

        builder.Property(c => c.Name).IsRequired().HasMaxLength(DishCatalog.NameMaxLength);

        // Unique among Approved and Suggested names only: rejected names can be suggested again
        builder.HasIndex(c => c.Name)
               .IsUnique()
               .HasFilter($"[Status] <> {(int)DishCatalogStatus.Rejected}");

        builder.Property(c => c.Category).IsRequired();
        builder.Property(c => c.Group).IsRequired();
        builder.Property(c => c.Status).IsRequired();
        builder.Property(c => c.SuggestedByUserId);
        builder.Property(c => c.ReviewedAtUtc);

        // Price ranges: one catalog name → one range per country, through the private list
        builder.HasMany(c => c.PriceRanges)
               .WithOne()
               .HasForeignKey(r => r.DishCatalogId)
               .IsRequired()
               .OnDelete(DeleteBehavior.Cascade);
        builder.Navigation(c => c.PriceRanges)
               .HasField("_priceRanges")
               .UsePropertyAccessMode(PropertyAccessMode.Field);

        builder.Ignore(c => c.CanBeUsed);   // calculated from Status

        builder.HasIndex(c => c.Status);    // admin: suggestions waiting for review
    }
}