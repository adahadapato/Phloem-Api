using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Phloem.Domain.Entities;
using Phloem.Persistence.Mappings;

namespace Phloem.Persistence.Configurations;

/// <summary>
/// Table <c>DeliveryPricings</c>: one row per country with the allowed range for shops' own delivery fees
/// (inclusive), plus the extra charge per delivery window in <c>DeliveryWindowCharges</c> (configured here, through
/// the private <c>_windowCharges</c> field).
/// </summary>
/// <remarks>
/// <para>One pricing per country (unique index). A country with no row can't take orders, so it's seeded.</para>
/// <para>The currency follows the country, so it isn't stored.</para>
/// </remarks>
public sealed class DeliveryPricingConfiguration : EntityConfiguration<DeliveryPricing>
{
    /// <inheritdoc />
    protected override void ConfigureEntity(EntityTypeBuilder<DeliveryPricing> builder)
    {
        builder.ToTable("DeliveryPricings");

        builder.Property(p => p.Country).IsRequired();
        builder.HasIndex(p => p.Country).IsUnique();

        builder.Property(p => p.MinShopFee).IsRequired().HasMoneyPrecision();
        builder.Property(p => p.MaxShopFee).IsRequired().HasMoneyPrecision();

        // Window charges: one per DeliveryWindow, through the private list
        builder.HasMany(p => p.WindowCharges)
               .WithOne()
               .HasForeignKey(c => c.DeliveryPricingId)
               .IsRequired()
               .OnDelete(DeleteBehavior.Cascade);
        builder.Navigation(p => p.WindowCharges)
               .HasField("_windowCharges")
               .UsePropertyAccessMode(PropertyAccessMode.Field);

        builder.Ignore(p => p.Currency);   // calculated from Country
    }
}