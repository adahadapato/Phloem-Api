using FluentValidation;
using Phloem.Application.Features.DishCatalogs.Commands;

namespace Phloem.Application.Features.DishCatalogs.Validators;

public sealed class SetDishPriceRangeCommandValidator : AbstractValidator<SetDishPriceRangeCommand>
{
    public SetDishPriceRangeCommandValidator()
    {
        RuleFor(x => x.DishCatalogId).NotEmpty();
        RuleFor(x => x.Country).IsInEnum();

        RuleFor(x => x.MinPrice).GreaterThan(0);

        RuleFor(x => x.MaxPrice)
            .GreaterThanOrEqualTo(x => x.MinPrice)
            .WithMessage("The maximum price can't be lower than the minimum.");
    }
}