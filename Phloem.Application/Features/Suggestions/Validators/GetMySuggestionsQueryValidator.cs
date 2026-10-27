using FluentValidation;
using Phloem.Application.Features.Suggestions.Queries;

namespace Phloem.Application.Features.Suggestions.Validators;

/// <summary>Input checks for <see cref="GetMySuggestionsQuery"/>.</summary>
public sealed class GetMySuggestionsQueryValidator : AbstractValidator<GetMySuggestionsQuery>
{
    /// <summary>A real country, and between 1 and <see cref="GetMySuggestionsQuery.MaxTake"/> dishes.</summary>
    public GetMySuggestionsQueryValidator()
    {
        RuleFor(x => x.Country).IsInEnum();
        RuleFor(x => x.Take).InclusiveBetween(1, GetMySuggestionsQuery.MaxTake);
    }
}
