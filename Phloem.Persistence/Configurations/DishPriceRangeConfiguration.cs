using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Phloem.Domain.Entities;

namespace Phloem.Persistence.Configurations;

/// <summary>
/// Table <c>DishPriceRanges</c>: the admin's allowed price range (inclusive) for one dish name in one country.
/// Child of <see cref="DishCatalog"/>; the relationship is configured in <see cref="DishCatalogConfiguration"/>.
/// </summary>
/// <remarks>One range per dish name per country (unique index). The currency follows the country, so it isn't stored.</remarks>
public sealed class DishPriceRangeConfiguration : EntityConfiguration<DishPriceRange>
{
    /// <inheritdoc />
    protected override void ConfigureEntity(EntityTypeBuilder<DishPriceRange> builder)
    {
        builder.ToTable("DishPriceRanges");

        builder.Property(r => r.DishCatalogId).IsRequired();
        builder.Property(r => r.Country).IsRequired();
        builder.Property(r => r.MinPrice).IsRequired().HasMoneyPrecision();
        builder.Property(r => r.MaxPrice).IsRequired().HasMoneyPrecision();

        builder.Ignore(r => r.Currency);   // calculated from Country

        builder.HasIndex(r => new { r.DishCatalogId, r.Country }).IsUnique();
    }
}