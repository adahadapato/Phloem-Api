using FluentValidation;
using Phloem.Application.Features.Dishes.Commands;
using Phloem.Domain.Entities;

namespace Phloem.Application.Features.Dishes.Validators;

public sealed class UpdateDishCommandValidator : AbstractValidator<UpdateDishCommand>
{
    public UpdateDishCommandValidator()
    {
        RuleFor(x => x.DishId).NotEmpty();
        RuleFor(x => x.DishCatalogId).NotEmpty().WithMessage("Please choose a dish name from the list.");
        RuleFor(x => x.Detail).MaximumLength(Dish.DetailMaxLength);
        RuleFor(x => x.Description).MaximumLength(Dish.DescriptionMaxLength);
        RuleFor(x => x.Price).GreaterThan(0);
    }
}