using FluentValidation;
using Phloem.Application.Features.DishCatalogs.Commands;

namespace Phloem.Application.Features.DishCatalogs.Validators;

public sealed class ApproveDishNameCommandValidator : AbstractValidator<ApproveDishNameCommand>
{
    public ApproveDishNameCommandValidator()
    {
        RuleFor(x => x.DishCatalogId).NotEmpty();
    }
}
