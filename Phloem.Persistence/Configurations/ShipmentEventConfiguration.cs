using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Phloem.Domain.Entities;

namespace Phloem.Persistence.Configurations;

/// <summary>
/// Table <c>ShipmentEvents</c>: one step in a parcel's tracking timeline ("Received at hub · 17 Oct 10:42 · Weighed:
/// 7.40 kg"). A history: rows are only ever added. Child of <see cref="Shipment"/>; the relationship is configured in
/// <see cref="ShipmentConfiguration"/>.
/// </summary>
public sealed class ShipmentEventConfiguration : EntityConfiguration<ShipmentEvent>
{
    /// <inheritdoc />
    protected override void ConfigureEntity(EntityTypeBuilder<ShipmentEvent> builder)
    {
        builder.ToTable("ShipmentEvents");

        builder.Property(e => e.ShipmentId).IsRequired();
        builder.Property(e => e.Status).IsRequired();
        builder.Property(e => e.OccurredAtUtc).IsRequired();
        builder.Property(e => e.Note).HasMaxLength(ShipmentEvent.NoteMaxLength);

        builder.HasIndex(e => new { e.ShipmentId, e.OccurredAtUtc });   // timeline in order
    }
}