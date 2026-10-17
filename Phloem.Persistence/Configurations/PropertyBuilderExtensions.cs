using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Phloem.Persistence.Configurations;

/// <summary>Column types used in many tables, so they're identical everywhere.</summary>
public static class PropertyBuilderExtensions
{
    /// <summary>Total digits for money columns.</summary>
    public const int MoneyPrecision = 18;

    /// <summary>Decimal places for money columns (pence / kobo / pesewas).</summary>
    public const int MoneyScale = 2;

    /// <summary>Total digits for weight columns (up to 9,999,999.99 kg).</summary>
    public const int WeightPrecision = 9;

    /// <summary>Decimal places for weight columns (kg).</summary>
    public const int WeightScale = 2;

    /// <summary>Money: decimal(18,2).</summary>
    /// <param name="property">The money property.</param>
    /// <returns>The same builder, for chaining.</returns>
    public static PropertyBuilder<decimal> HasMoneyPrecision(this PropertyBuilder<decimal> property) =>
        property.HasPrecision(MoneyPrecision, MoneyScale);

    /// <summary>Optional money: decimal(18,2).</summary>
    /// <param name="property">The money property.</param>
    /// <returns>The same builder, for chaining.</returns>
    public static PropertyBuilder<decimal?> HasMoneyPrecision(this PropertyBuilder<decimal?> property) =>
        property.HasPrecision(MoneyPrecision, MoneyScale);

    /// <summary>Weight in kg: decimal(9,2).</summary>
    /// <param name="property">The weight property.</param>
    /// <returns>The same builder, for chaining.</returns>
    public static PropertyBuilder<decimal> HasWeightPrecision(this PropertyBuilder<decimal> property) =>
        property.HasPrecision(WeightPrecision, WeightScale);

    /// <summary>Optional weight in kg: decimal(9,2).</summary>
    /// <param name="property">The weight property.</param>
    /// <returns>The same builder, for chaining.</returns>
    public static PropertyBuilder<decimal?> HasWeightPrecision(this PropertyBuilder<decimal?> property) =>
        property.HasPrecision(WeightPrecision, WeightScale);
}