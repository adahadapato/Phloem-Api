using FluentValidation;
using Phloem.Application.Features.DeliveryPricings.Commands;

namespace Phloem.Application.Features.DeliveryPricings.Validators;

/// <summary>Input checks for <see cref="SetDeliveryPricingCommand"/>.</summary>
public sealed class SetDeliveryPricingCommandValidator : AbstractValidator<SetDeliveryPricingCommand>
{
    /// <summary>Country must be real; min 0 or more; max at least min.</summary>
    public SetDeliveryPricingCommandValidator()
    {
        RuleFor(x => x.Country).IsInEnum();
        RuleFor(x => x.MinShopFee).GreaterThanOrEqualTo(0);
        RuleFor(x => x.MaxShopFee)
            .GreaterThanOrEqualTo(x => x.MinShopFee)
            .WithMessage("The maximum fee can't be lower than the minimum.");
    }
}