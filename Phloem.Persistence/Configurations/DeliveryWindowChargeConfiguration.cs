using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Phloem.Domain.Entities;

namespace Phloem.Persistence.Configurations;

/// <summary>
/// Table <c>DeliveryWindowCharges</c>: what one delivery window (ASAP / this evening / tomorrow) adds to the shop's fee
/// in one country. A window with no row isn't offered there. Child of <see cref="DeliveryPricing"/>; the relationship is
/// configured in <see cref="DeliveryPricingConfiguration"/>.
/// </summary>
/// <remarks>One charge per window per country (unique index).</remarks>
public sealed class DeliveryWindowChargeConfiguration : EntityConfiguration<DeliveryWindowCharge>
{
    /// <inheritdoc />
    protected override void ConfigureEntity(EntityTypeBuilder<DeliveryWindowCharge> builder)
    {
        builder.ToTable("DeliveryWindowCharges");

        builder.Property(c => c.DeliveryPricingId).IsRequired();
        builder.Property(c => c.Window).IsRequired();
        builder.Property(c => c.ExtraCharge).IsRequired().HasMoneyPrecision();

        builder.HasIndex(c => new { c.DeliveryPricingId, c.Window }).IsUnique();
    }
}