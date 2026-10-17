using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Phloem.Domain.Entities;

namespace Phloem.Persistence.Configurations;

/// <summary>
/// Table <c>ShipmentRates</c>: the price (base charge + per chargeable kg, origin currency) and typical transit days for
/// one speed on one route. Child of <see cref="ShipmentRoute"/>; the relationship is configured in
/// <see cref="ShipmentRouteConfiguration"/>.
/// </summary>
/// <remarks>One rate per speed per route (unique index). A speed with no row isn't offered on that route.</remarks>
public sealed class ShipmentRateConfiguration : EntityConfiguration<ShipmentRate>
{
    /// <inheritdoc />
    protected override void ConfigureEntity(EntityTypeBuilder<ShipmentRate> builder)
    {
        builder.ToTable("ShipmentRates");

        builder.Property(r => r.ShipmentRouteId).IsRequired();
        builder.Property(r => r.Speed).IsRequired();
        builder.Property(r => r.BaseCharge).IsRequired().HasMoneyPrecision();
        builder.Property(r => r.PricePerKg).IsRequired().HasMoneyPrecision();
        builder.Property(r => r.MinTransitDays).IsRequired();
        builder.Property(r => r.MaxTransitDays).IsRequired();

        builder.HasIndex(r => new { r.ShipmentRouteId, r.Speed }).IsUnique();
    }
}