using FluentValidation;
using Phloem.Application.Features.Suggestions.Queries;

namespace Phloem.Application.Features.Suggestions.Validators;

/// <summary>Input checks for <see cref="GetPopularDishesQuery"/>.</summary>
public sealed class GetPopularDishesQueryValidator : AbstractValidator<GetPopularDishesQuery>
{
    /// <summary>A real country, and between 1 and <see cref="GetMySuggestionsQuery.MaxTake"/> dishes.</summary>
    public GetPopularDishesQueryValidator()
    {
        RuleFor(x => x.Country).IsInEnum();
        RuleFor(x => x.Take).InclusiveBetween(1, GetMySuggestionsQuery.MaxTake);
    }
}
