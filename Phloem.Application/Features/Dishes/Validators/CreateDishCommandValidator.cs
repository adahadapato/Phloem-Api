using FluentValidation;
using Phloem.Application.Features.Dishes.Commands;
using Phloem.Domain.Entities;

namespace Phloem.Application.Features.Dishes.Validators;

public sealed class CreateDishCommandValidator : AbstractValidator<CreateDishCommand>
{
    public CreateDishCommandValidator()
    {
        RuleFor(x => x.DishCatalogId).NotEmpty().WithMessage("Please choose a dish name from the list.");
        RuleFor(x => x.Detail).MaximumLength(Dish.DetailMaxLength);
        RuleFor(x => x.Description).MaximumLength(Dish.DescriptionMaxLength);

        // Only "above zero" here: the RANGE is the Domain's rule (it depends on the database)
        RuleFor(x => x.Price).GreaterThan(0);
    }
}