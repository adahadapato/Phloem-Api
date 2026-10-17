using FluentValidation;

namespace Phloem.Application.Features.Orders.Validators;

/// <summary>Order-number rule shared by every validator that takes one.</summary>
internal static class OrderNumberRules
{
    /// <summary>Longer than any real order number ("PH-261016-482193" is 16).</summary>
    public const int MaxLength = 30;

    /// <summary>Applies the rule.</summary>
    /// <typeparam name="T">The command or query.</typeparam>
    /// <param name="rule">The rule builder for the OrderNumber property.</param>
    public static void Apply<T>(IRuleBuilderInitial<T, string> rule) =>
        rule.NotEmpty().MaximumLength(MaxLength);
}
