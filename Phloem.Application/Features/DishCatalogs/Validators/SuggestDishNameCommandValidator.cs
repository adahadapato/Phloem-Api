using FluentValidation;
using Phloem.Application.Features.DishCatalogs.Commands;
using Phloem.Domain.Entities;

namespace Phloem.Application.Features.DishCatalogs.Validators;

public sealed class SuggestDishNameCommandValidator : AbstractValidator<SuggestDishNameCommand>
{
    public SuggestDishNameCommandValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(DishCatalog.NameMaxLength);
        RuleFor(x => x.Category).IsInEnum();
        RuleFor(x => x.Group).IsInEnum();
    }
}