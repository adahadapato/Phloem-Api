using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Phloem.Domain.Entities;

namespace Phloem.Persistence.Configurations;

/// <summary>
/// Table <c>ShipmentRoutes</c>: one shipping direction (e.g. Ghana → United Kingdom) with its drop-off hub and on/off
/// switch, and its prices per speed in <c>ShipmentRates</c> (configured here, through the private <c>_rates</c> field).
/// </summary>
/// <remarks>
/// <para>One route per direction (unique index on From + To); Ghana → UK and UK → Ghana are two rows.</para>
/// <para>The currency follows the origin country, so it isn't stored.</para>
/// </remarks>
public sealed class ShipmentRouteConfiguration : EntityConfiguration<ShipmentRoute>
{
    /// <inheritdoc />
    protected override void ConfigureEntity(EntityTypeBuilder<ShipmentRoute> builder)
    {
        builder.ToTable("ShipmentRoutes");

        builder.Property(r => r.FromCountry).IsRequired();
        builder.Property(r => r.ToCountry).IsRequired();
        builder.HasIndex(r => new { r.FromCountry, r.ToCountry }).IsUnique();

        builder.Property(r => r.DropOffAddress).IsRequired().HasMaxLength(ShipmentRoute.DropOffAddressMaxLength);
        builder.Property(r => r.DropOffHours).IsRequired().HasMaxLength(ShipmentRoute.DropOffHoursMaxLength);
        builder.Property(r => r.IsActive).IsRequired();

        // Rates: one per speed, through the private list
        builder.HasMany(r => r.Rates)
               .WithOne()
               .HasForeignKey(rate => rate.ShipmentRouteId)
               .IsRequired()
               .OnDelete(DeleteBehavior.Cascade);
        builder.Navigation(r => r.Rates)
               .HasField("_rates")
               .UsePropertyAccessMode(PropertyAccessMode.Field);

        builder.Ignore(r => r.Currency);    // calculated from FromCountry

        builder.HasIndex(r => r.IsActive);  // "Send a parcel": switched-on routes
    }
}