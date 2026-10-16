using FluentValidation;
using Phloem.Application.Features.DeliveryPricings.Commands;

namespace Phloem.Application.Features.DeliveryPricings.Validators;

/// <summary>
/// Input checks for <see cref="SetMyDeliveryFeeCommand"/>. Only "0 or more" here: the RANGE is the
/// Domain's rule, because it depends on the country's pricing in the database.
/// </summary>
public sealed class SetMyDeliveryFeeCommandValidator : AbstractValidator<SetMyDeliveryFeeCommand>
{
    /// <summary>The fee can't be negative.</summary>
    public SetMyDeliveryFeeCommandValidator()
    {
        RuleFor(x => x.Fee).GreaterThanOrEqualTo(0);
    }
}