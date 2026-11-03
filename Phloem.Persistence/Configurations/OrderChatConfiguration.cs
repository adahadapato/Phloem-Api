using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Phloem.Domain.Entities;

namespace Phloem.Persistence.Configurations;

/// <summary>Table <c>OrderChats</c>: one conversation per order.</summary>
public sealed class OrderChatConfiguration : EntityConfiguration<OrderChat>
{
    /// <inheritdoc />
    protected override void ConfigureEntity(EntityTypeBuilder<OrderChat> builder)
    {
        builder.ToTable("OrderChats");

        builder.Property(c => c.OrderId).IsRequired();
        builder.Property(c => c.OrderNumber).IsRequired().HasMaxLength(Order.OrderNumberMaxLength);
        builder.Property(c => c.CustomerId).IsRequired();
        builder.Property(c => c.ShopId).IsRequired();
        builder.Property(c => c.ShopOwnerId).IsRequired();
        builder.Property(c => c.ShopName).IsRequired().HasMaxLength(Vendor.NameMaxLength);
        builder.Property(c => c.Country).IsRequired();
        builder.Property(c => c.ReportReason).HasMaxLength(OrderChat.ReportReasonMaxLength);
        builder.Ignore(c => c.LastMessageAtUtc);
        builder.Ignore(c => c.ReportWaiting);

        builder.HasIndex(c => c.OrderId).IsUnique();
        builder.HasIndex(c => new { c.CustomerId, c.LastCustomerMessageAtUtc });
        builder.HasIndex(c => new { c.ShopOwnerId, c.LastSellerMessageAtUtc });
        builder.HasIndex(c => new { c.IsReported, c.ReportHandledAtUtc });
    }
}
