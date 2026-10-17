using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Phloem.Domain.Entities;
using Phloem.Domain.ValueObjects;
using Phloem.Persistence.Mappings;

namespace Phloem.Persistence.Configurations;

/// <summary>
/// Table <c>OrderLines</c>: one dish in an order, with the dish's name, photo and price COPIED at the time (so the
/// order history never changes when the dish does). Child of <see cref="Order"/>; the relationship is configured in
/// <see cref="OrderConfiguration"/>.
/// </summary>
/// <remarks>
/// <para><c>DishId</c> has a foreign key to <c>Dishes</c> with Restrict: dishes are soft-deleted, never removed, so a
/// line always points at a real row. There's no navigation property; lines carry their own snapshot.</para>
/// <para><c>DishImagePath</c> points at a stored photo file that must never be deleted (legal / order history).</para>
/// <para><c>LastNotifiedPrice</c> remembers the price already emailed about, so nobody is told twice (weekly email).</para>
/// <para><c>LineTotal</c> is calculated, not stored.</para>
/// </remarks>
public sealed class OrderLineConfiguration : EntityConfiguration<OrderLine>
{
    /// <inheritdoc />
    protected override void ConfigureEntity(EntityTypeBuilder<OrderLine> builder)
    {
        builder.ToTable("OrderLines");

        builder.Property(l => l.OrderId).IsRequired();

        builder.HasOne<Dish>()
               .WithMany()
               .HasForeignKey(l => l.DishId)
               .IsRequired()
               .OnDelete(DeleteBehavior.Restrict);

        builder.Property(l => l.DishName).IsRequired().HasMaxLength(Dish.NameMaxLength);
        builder.Property(l => l.DishImagePath).HasMaxLength(ImageFile.PathMaxLength);
        builder.Property(l => l.UnitPrice).IsRequired().HasMoneyPrecision();
        builder.Property(l => l.LastNotifiedPrice).IsRequired().HasMoneyPrecision();
        builder.Property(l => l.Quantity).IsRequired();

        builder.Ignore(l => l.LineTotal);   // calculated

        builder.HasIndex(l => l.OrderId);
        builder.HasIndex(l => l.DishId);    // "Buy again", unpaid orders containing a dish
    }
}