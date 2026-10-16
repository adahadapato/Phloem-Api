using FluentValidation;
using Phloem.Application.Features.Dishes.Commands;

namespace Phloem.Application.Features.Dishes.Validators;

public sealed class ChangeDishPriceCommandValidator : AbstractValidator<ChangeDishPriceCommand>
{
    public ChangeDishPriceCommandValidator()
    {
        RuleFor(x => x.DishId).NotEmpty();
        RuleFor(x => x.Price).GreaterThan(0);
    }
}