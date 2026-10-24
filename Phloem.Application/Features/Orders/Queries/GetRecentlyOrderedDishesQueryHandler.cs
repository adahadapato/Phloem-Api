using MediatR;
using Phloem.Application.Features.Dishes.Dtos;
using Phloem.Domain.Interfaces;
using Phloem.Domain.Shared.Results;

namespace Phloem.Application.Features.Orders.Queries;

/// <summary>Handles <see cref="GetRecentlyOrderedDishesQuery"/>.</summary>
public sealed class GetRecentlyOrderedDishesQueryHandler(
    ICurrentUser currentUser,
    IOrderRepository orders,
    IDishRepository dishes,
    IFileStorage storage)
    : IRequestHandler<GetRecentlyOrderedDishesQuery, Result<IReadOnlyList<DishCardDto>>>
{
    /// <summary>
    /// Takes the newest dish Ids from the customer's paid orders (a few extra, as some may no longer be on sale),
    /// loads them in ONE query, keeps those that can be ordered now, in the order they were last bought.
    /// </summary>
    /// <exception cref="InvalidOperationException">Called without a signed-in user (endpoint misconfigured).</exception>
    public async Task<Result<IReadOnlyList<DishCardDto>>> Handle(GetRecentlyOrderedDishesQuery query,
                                                                  CancellationToken ct)
    {
        var userId = currentUser.UserId
            ?? throw new InvalidOperationException("This action requires a signed-in user.");

        var ids = await orders.GetRecentDishIdsForCustomerAsync(userId, query.Take * 2, ct);
        if (ids.Count == 0) return Result.Success<IReadOnlyList<DishCardDto>>([]);

        var found = (await dishes.GetByIdsAsync(ids, ct)).ToDictionary(d => d.Id);

        IReadOnlyList<DishCardDto> cards = ids
            .Select(id => found.GetValueOrDefault(id))
            .Where(d => d is { CanBeOrdered: true })
            .Take(query.Take)
            .Select(d => DishCardDto.From(d!, storage))
            .ToList();

        return Result.Success(cards);
    }
}
