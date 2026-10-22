using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Phloem.Domain.Entities;
using Phloem.Persistence.Mappings;

namespace Phloem.Persistence.Configurations;

/// <summary>
/// Table <c>Orders</c>: one customer's order from ONE shop, with its lines in <c>OrderLines</c> (configured here,
/// through the private <c>_lines</c> field).
/// </summary>
/// <remarks>
/// <para><b>Snapshots:</b> the shop name and delivery address are COPIED when the order is placed, so the order still
/// reads correctly after the shop renames or the customer edits/deletes the address.</para>
/// <para><b>Links:</b> foreign keys to the customer (UserProfile) and the shop (Vendor) with Restrict: both are
/// soft-deleted, never removed, so old orders always point at real rows. Repositories must NOT <c>Include</c>
/// <c>Customer</c> (a deleted profile is filtered out and would hide the order); names come from
/// <c>GetFullNamesAsync</c>.</para>
/// <para><b>Money</b> is in the shop's currency, decimal(18,2). <c>Total</c>, <c>IsPaid</c> and <c>NeedsRefund</c> are
/// calculated, not stored.</para>
/// </remarks>
public sealed class OrderConfiguration : EntityConfiguration<Order>
{
    /// <inheritdoc />
    protected override void ConfigureEntity(EntityTypeBuilder<Order> builder)
    {
        builder.ToTable("Orders");

        // ---------- Identity ----------
        builder.Property(o => o.OrderNumber).IsRequired().HasMaxLength(Order.OrderNumberMaxLength);
        builder.HasIndex(o => o.OrderNumber).IsUnique();

        // ---------- Customer & shop ----------
        builder.HasOne(o => o.Customer)
               .WithMany()
               .HasForeignKey(o => o.CustomerId)
               .IsRequired()
               .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne<Vendor>()
               .WithMany()
               .HasForeignKey(o => o.VendorId)
               .IsRequired()
               .OnDelete(DeleteBehavior.Restrict);

        builder.Property(o => o.VendorName).IsRequired().HasMaxLength(Vendor.NameMaxLength);

        // ---------- Delivery (snapshot) ----------
        builder.Property(o => o.DeliveryLabel).IsRequired().HasMaxLength(DeliveryAddress.LabelMaxLength);
        builder.Property(o => o.DeliveryAddressLine).IsRequired().HasMaxLength(DeliveryAddress.FullAddressMaxLength);
        builder.Property(o => o.DeliveryCountry).IsRequired();
        builder.Property(o => o.DeliveryWindow).IsRequired();

        // ---------- Money ----------
        builder.Property(o => o.Currency).IsRequired();
        builder.Property(o => o.Subtotal).IsRequired().HasMoneyPrecision();
        builder.Property(o => o.DeliveryFee).IsRequired().HasMoneyPrecision();
        builder.Property(o => o.CommissionAmount).HasMoneyPrecision();   // set on delivery
        builder.Property(o => o.SellerEarnings).HasMoneyPrecision();     // set on delivery

        // ---------- Status & dates ----------
        builder.Property(o => o.Status).IsRequired();
        builder.Property(o => o.PlacedAtUtc).IsRequired();
        builder.Property(o => o.PaidAtUtc);
        builder.Property(o => o.PaymentReference).HasMaxLength(Order.PaymentReferenceMaxLength);
        builder.Property(o => o.CancelledAtUtc);
        builder.Property(o => o.DeclinedByShop).IsRequired();
        builder.Property(o => o.DeclineReason).HasMaxLength(Order.DeclineReasonMaxLength);
        builder.Property(o => o.DeliveredAtUtc);

        // ---------- Lines ----------
        builder.HasMany(o => o.Lines)
               .WithOne()
               .HasForeignKey(l => l.OrderId)
               .IsRequired()
               .OnDelete(DeleteBehavior.Cascade);
        builder.Navigation(o => o.Lines)
               .HasField("_lines")
               .UsePropertyAccessMode(PropertyAccessMode.Field);

        // ---------- Calculated, not columns ----------
        builder.Ignore(o => o.Total);
        builder.Ignore(o => o.IsPaid);
        builder.Ignore(o => o.NeedsRefund);

        // ---------- Lists ----------
        builder.HasIndex(o => new { o.CustomerId, o.PlacedAtUtc });             // "My orders", newest first
        builder.HasIndex(o => new { o.VendorId, o.Status, o.PaidAtUtc });       // shop orders + daily sales
        builder.HasIndex(o => new { o.Status, o.PlacedAtUtc });                 // unpaid orders (weekly price email)
        builder.HasIndex(o => o.PaymentReference);                              // payment callbacks look orders up by it
    }
}