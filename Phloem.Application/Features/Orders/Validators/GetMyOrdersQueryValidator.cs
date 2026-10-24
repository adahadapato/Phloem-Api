using FluentValidation;
using Phloem.Application.Features.Orders.Queries;
using Phloem.Domain.Models;

namespace Phloem.Application.Features.Orders.Validators;

/// <summary>Input checks for <see cref="GetMyOrdersQuery"/>.</summary>
public sealed class GetMyOrdersQueryValidator : AbstractValidator<GetMyOrdersQuery>
{
    /// <summary>A real tab, a start date not in the future, and a short search text.</summary>
    /// <param name="time">Today's date, for the start date check.</param>
    public GetMyOrdersQueryValidator(TimeProvider time)
    {
        RuleFor(x => x.Show).IsInEnum();

        RuleFor(x => x.From)
            .Must(from => from <= DateOnly.FromDateTime(time.GetUtcNow().UtcDateTime))
            .WithMessage("The start date can't be in the future.")
            .When(x => x.From is not null);

        RuleFor(x => x.Search).MaximumLength(CustomerOrderFilter.SearchMaxLength);
    }
}
