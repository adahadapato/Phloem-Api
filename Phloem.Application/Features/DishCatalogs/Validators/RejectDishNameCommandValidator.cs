using FluentValidation;
using Phloem.Application.Features.DishCatalogs.Commands;

namespace Phloem.Application.Features.DishCatalogs.Validators;

public sealed class RejectDishNameCommandValidator : AbstractValidator<RejectDishNameCommand>
{
    public RejectDishNameCommandValidator()
    {
        RuleFor(x => x.DishCatalogId).NotEmpty();
    }
}
