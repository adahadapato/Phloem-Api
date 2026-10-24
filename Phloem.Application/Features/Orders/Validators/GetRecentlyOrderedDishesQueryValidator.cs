using FluentValidation;
using Phloem.Application.Features.Orders.Queries;

namespace Phloem.Application.Features.Orders.Validators;

/// <summary>Input checks for <see cref="GetRecentlyOrderedDishesQuery"/>.</summary>
public sealed class GetRecentlyOrderedDishesQueryValidator : AbstractValidator<GetRecentlyOrderedDishesQuery>
{
    /// <summary>Between 1 and <see cref="GetRecentlyOrderedDishesQuery.MaxTake"/> dishes.</summary>
    public GetRecentlyOrderedDishesQueryValidator()
    {
        RuleFor(x => x.Take).InclusiveBetween(1, GetRecentlyOrderedDishesQuery.MaxTake);
    }
}
