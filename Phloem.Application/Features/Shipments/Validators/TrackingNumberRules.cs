using FluentValidation;

namespace Phloem.Application.Features.Shipments.Validators;

/// <summary>Tracking-number rule shared by every validator that takes one.</summary>
internal static class TrackingNumberRules
{
    /// <summary>Longer than any real tracking number ("PHS-261017-482193" is 17).</summary>
    public const int MaxLength = 30;

    /// <summary>Applies the rule.</summary>
    /// <typeparam name="T">The command or query.</typeparam>
    /// <param name="rule">The rule builder for the TrackingNumber property.</param>
    public static void Apply<T>(IRuleBuilderInitial<T, string> rule) =>
        rule.NotEmpty().MaximumLength(MaxLength);
}
