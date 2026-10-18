using FluentValidation;
using Phloem.Application.Features.Vendors.Queries;

namespace Phloem.Application.Features.Vendors.Validators;

public sealed class GetShopsQueryValidator : AbstractValidator<GetShopsQuery>
{
    /// <summary>Longest search text accepted (same as the dish search).</summary>
    public const int SearchMaxLength = 100;

    public GetShopsQueryValidator()
    {
        RuleFor(x => x.Country).IsInEnum().When(x => x.Country is not null);
        RuleFor(x => x.Search).MaximumLength(SearchMaxLength);
    }
}
