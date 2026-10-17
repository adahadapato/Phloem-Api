using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Phloem.Domain.Entities;

namespace Phloem.Persistence.Configurations;

/// <summary>
/// Table <c>Commissions</c>: Phloem's commission rate per country, taken from the dishes subtotal of delivered orders
/// (never from the delivery fee). A country with no row pays 0%.
/// </summary>
/// <remarks>
/// <para>One rate per country (unique index).</para>
/// <para><c>RatePercent</c> is stored as decimal(5,2), e.g. 12.50, which covers the Domain's 0 to
/// <see cref="Commission.MaxRatePercent"/>% with two decimal places.</para>
/// </remarks>
public sealed class CommissionConfiguration : EntityConfiguration<Commission>
{
    /// <summary>Total digits for a percentage (up to 999.99).</summary>
    private const int PercentPrecision = 5;

    /// <summary>Decimal places for a percentage.</summary>
    private const int PercentScale = 2;

    /// <inheritdoc />
    protected override void ConfigureEntity(EntityTypeBuilder<Commission> builder)
    {
        builder.ToTable("Commissions");

        builder.Property(c => c.Country).IsRequired();
        builder.HasIndex(c => c.Country).IsUnique();

        builder.Property(c => c.RatePercent).IsRequired().HasPrecision(PercentPrecision, PercentScale);
    }
}