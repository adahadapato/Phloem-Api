using MediatR;
using Phloem.Application.Features.Dishes.Dtos;
using Phloem.Domain.Shared.Results;

namespace Phloem.Application.Features.Orders.Queries;

/// <summary>
/// "Buy it again" panel beside My orders: dishes from the customer's paid orders that can still be ordered,
/// most recently ordered first, as the same cards Explore shows (so "+" puts them straight in the basket).
/// Never cached (personal).
/// </summary>
/// <param name="Take">How many dishes at most (1–20).</param>
public sealed record GetRecentlyOrderedDishesQuery(int Take = GetRecentlyOrderedDishesQuery.DefaultTake)
    : IRequest<Result<IReadOnlyList<DishCardDto>>>
{
    /// <summary>How many dishes when the caller doesn't say.</summary>
    public const int DefaultTake = 8;

    /// <summary>Most dishes one call returns.</summary>
    public const int MaxTake = 20;
}
