using FluentValidation;
using Phloem.Application.Features.DeliveryPricings.Commands;

namespace Phloem.Application.Features.DeliveryPricings.Validators;

/// <summary>Input checks for <see cref="RemoveDeliveryWindowCommand"/>.</summary>
public sealed class RemoveDeliveryWindowCommandValidator : AbstractValidator<RemoveDeliveryWindowCommand>
{
    /// <summary>Country and window must be real values.</summary>
    public RemoveDeliveryWindowCommandValidator()
    {
        RuleFor(x => x.Country).IsInEnum();
        RuleFor(x => x.Window).IsInEnum();
    }
}