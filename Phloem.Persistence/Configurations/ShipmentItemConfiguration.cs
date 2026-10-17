using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Phloem.Domain.Entities;
using Phloem.Persistence.Mappings;

namespace Phloem.Persistence.Configurations;

/// <summary>
/// Table <c>ShipmentItems</c>: one item the sender declared for customs (category, description, quantity, value of one).
/// Never changed after booking. Child of <see cref="Shipment"/>; the relationship is configured in
/// <see cref="ShipmentConfiguration"/>.
/// </summary>
/// <remarks><c>TotalDeclaredValue</c> (value × quantity) is calculated, not stored.</remarks>
public sealed class ShipmentItemConfiguration : EntityConfiguration<ShipmentItem>
{
    /// <inheritdoc />
    protected override void ConfigureEntity(EntityTypeBuilder<ShipmentItem> builder)
    {
        builder.ToTable("ShipmentItems");

        builder.Property(i => i.ShipmentId).IsRequired();
        builder.Property(i => i.Category).IsRequired();
        builder.Property(i => i.Description).IsRequired().HasMaxLength(ShipmentItem.DescriptionMaxLength);
        builder.Property(i => i.Quantity).IsRequired();
        builder.Property(i => i.UnitDeclaredValue).IsRequired().HasMoneyPrecision();

        builder.Ignore(i => i.TotalDeclaredValue);

        builder.HasIndex(i => i.ShipmentId);
    }
}