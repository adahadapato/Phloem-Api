using FluentValidation;
using Phloem.Application.Features.DeliveryPricings.Commands;

namespace Phloem.Application.Features.DeliveryPricings.Validators;

/// <summary>Input checks for <see cref="SetDeliveryWindowChargeCommand"/>.</summary>
public sealed class SetDeliveryWindowChargeCommandValidator : AbstractValidator<SetDeliveryWindowChargeCommand>
{
    /// <summary>Country and window must be real; the extra charge 0 or more.</summary>
    public SetDeliveryWindowChargeCommandValidator()
    {
        RuleFor(x => x.Country).IsInEnum();
        RuleFor(x => x.Window).IsInEnum();
        RuleFor(x => x.ExtraCharge).GreaterThanOrEqualTo(0);
    }
}