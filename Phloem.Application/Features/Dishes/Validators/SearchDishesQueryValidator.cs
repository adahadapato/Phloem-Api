using FluentValidation;
using Phloem.Application.Features.Dishes.Queries;

namespace Phloem.Application.Features.Dishes.Validators;

public sealed class SearchDishesQueryValidator : AbstractValidator<SearchDishesQuery>
{
    public SearchDishesQueryValidator()
    {
        RuleFor(x => x.Country).IsInEnum().When(x => x.Country is not null);
        RuleFor(x => x.Category).IsInEnum().When(x => x.Category is not null);
        RuleFor(x => x.Sort).IsInEnum();
        RuleFor(x => x.Search).MaximumLength(100);
    }
}