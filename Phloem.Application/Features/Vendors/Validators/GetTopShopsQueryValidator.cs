using FluentValidation;
using Phloem.Application.Features.Vendors.Queries;

namespace Phloem.Application.Features.Vendors.Validators;

public sealed class GetTopShopsQueryValidator : AbstractValidator<GetTopShopsQuery>
{
    public GetTopShopsQueryValidator()
    {
        RuleFor(x => x.Country).IsInEnum().When(x => x.Country is not null);
        RuleFor(x => x.Count).InclusiveBetween(1, GetTopShopsQuery.MaxCount);
    }
}
