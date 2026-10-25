using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Phloem.Domain.Entities;
using Phloem.Persistence.Converters;

namespace Phloem.Persistence.Configurations;

/// <summary>
/// Table <c>DeliveryAddresses</c>: a customer's saved addresses. Child of <see cref="UserProfile"/>; the relationship
/// is configured in <see cref="UserProfileConfiguration"/>.
/// </summary>
/// <remarks>
/// <para>Removing an address really deletes the row (it isn't soft-deleted). Orders are safe because they copy the
/// address text when placed.</para>
/// <para>"Exactly one default per user" is enforced by UserProfile, not by a database index.</para>
/// </remarks>
public sealed class DeliveryAddressConfiguration : EntityConfiguration<DeliveryAddress>
{
    /// <inheritdoc />
    protected override void ConfigureEntity(EntityTypeBuilder<DeliveryAddress> builder)
    {
        builder.ToTable("DeliveryAddresses");

        builder.Property(a => a.UserId).IsRequired();
        builder.Property(a => a.Label).IsRequired().HasMaxLength(DeliveryAddress.LabelMaxLength);
        builder.Property(a => a.FullAddress).IsRequired().HasMaxLength(DeliveryAddress.FullAddressMaxLength);
        builder.Property(a => a.Country).IsRequired();
        builder.Property(a => a.IsDefault).IsRequired();

        // Optional: who receives it and who to call (null = the account holder). The phone's converter and size
        // come from PhloemDbContext.ConfigureConventions.
        builder.Property(a => a.RecipientName).HasMaxLength(DeliveryAddress.RecipientNameMaxLength);
        builder.Property(a => a.PhoneNumber).HasMaxLength(PhoneNumberConverter.MaxLength);

        builder.HasIndex(a => a.UserId);   // "my addresses"
    }
}
