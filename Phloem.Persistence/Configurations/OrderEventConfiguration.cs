using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Phloem.Domain.Entities;

namespace Phloem.Persistence.Configurations;

/// <summary>Table <c>OrderEvents</c>: each order's timeline (never changed or deleted).</summary>
public sealed class OrderEventConfiguration : EntityConfiguration<OrderEvent>
{
    /// <inheritdoc />
    protected override void ConfigureEntity(EntityTypeBuilder<OrderEvent> builder)
    {
        builder.ToTable("OrderEvents");

        builder.Property(e => e.OrderId).IsRequired();
        builder.Property(e => e.Kind).IsRequired();
        builder.Property(e => e.StatusAfter).IsRequired();
        builder.Property(e => e.OccurredAtUtc).IsRequired();
        builder.Property(e => e.ActorKind).IsRequired();
        builder.Property(e => e.ActorId);
        builder.Property(e => e.Note).HasMaxLength(OrderEvent.NoteMaxLength);
        builder.Property(e => e.IsRebuilt).IsRequired().HasDefaultValue(false);

        builder.HasIndex(e => new { e.OrderId, e.OccurredAtUtc });   // one order's timeline, in order
        builder.HasIndex(e => new { e.ActorId, e.OccurredAtUtc });   // "what did this person do?" (disputes)
    }
}
