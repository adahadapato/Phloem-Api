using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Phloem.Domain.Entities;

namespace Phloem.Persistence.Configurations;

/// <summary>Table <c>ParcelItemRules</c>: the "what can be sent" list shown when booking a parcel.</summary>
/// <remarks>Read in <c>SortOrder</c> order (indexed). Lines are really deleted, so the soft-delete columns stay
/// unused here.</remarks>
public sealed class ParcelItemRuleConfiguration : EntityConfiguration<ParcelItemRule>
{
    /// <inheritdoc />
    protected override void ConfigureEntity(EntityTypeBuilder<ParcelItemRule> builder)
    {
        builder.ToTable("ParcelItemRules");

        builder.Property(r => r.Kind).IsRequired();
        builder.Property(r => r.Name).IsRequired().HasMaxLength(ParcelItemRule.NameMaxLength);
        builder.Property(r => r.Detail).HasMaxLength(ParcelItemRule.DetailMaxLength);
        builder.Property(r => r.SortOrder).IsRequired();
        builder.HasIndex(r => r.SortOrder);
    }
}
