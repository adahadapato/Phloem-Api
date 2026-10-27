using FluentValidation;
using Phloem.Application.Features.Dishes.Queries;

namespace Phloem.Application.Features.Dishes.Validators;

/// <summary>Input checks for <see cref="SuggestSearchQuery"/>.</summary>
public sealed class SuggestSearchQueryValidator : AbstractValidator<SuggestSearchQuery>
{
    public SuggestSearchQueryValidator()
    {
        RuleFor(x => x.Q).NotEmpty()
                         .Must(q => q is not null && q.Trim().Length >= SuggestSearchQuery.MinLength)
                         .WithMessage($"Type at least {SuggestSearchQuery.MinLength} letters.")
                         .MaximumLength(SuggestSearchQuery.MaxLength);
        RuleFor(x => x.Country).IsInEnum().When(x => x.Country is not null);
        RuleFor(x => x.Take).InclusiveBetween(1, SuggestSearchQuery.MaxTake);
    }
}
