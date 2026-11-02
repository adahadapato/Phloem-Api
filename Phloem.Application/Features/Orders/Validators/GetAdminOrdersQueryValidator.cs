using FluentValidation;
using Phloem.Application.Features.Orders.Queries;

namespace Phloem.Application.Features.Orders.Validators;

/// <summary>Input checks for <see cref="GetAdminOrdersQuery"/>.</summary>
public sealed class GetAdminOrdersQueryValidator : AbstractValidator<GetAdminOrdersQuery>
{
    /// <summary>Filters must be real values when given; "from" can't be after "to".</summary>
    public GetAdminOrdersQueryValidator()
    {
        RuleFor(x => x.Country).IsInEnum();
        RuleFor(x => x.Status).IsInEnum();
        RuleFor(x => x.Search).MaximumLength(100);
        RuleFor(x => x.To)
            .GreaterThanOrEqualTo(x => x.From)
            .When(x => x.From is not null && x.To is not null)
            .WithMessage("'To' can't be before 'From'.");
    }
}
