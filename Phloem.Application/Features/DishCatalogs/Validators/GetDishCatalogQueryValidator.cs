using FluentValidation;
using Phloem.Application.Features.DishCatalogs.Queries;

namespace Phloem.Application.Features.DishCatalogs.Validators;

public sealed class GetDishCatalogQueryValidator : AbstractValidator<GetDishCatalogQuery>
{
    public GetDishCatalogQueryValidator()
    {
        RuleFor(x => x.Country).IsInEnum();
    }
}