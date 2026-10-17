using MediatR;
using Phloem.Application.Features.Orders.Dtos;
using Phloem.Domain.Interfaces;
using Phloem.Domain.Shared.Errors;
using Phloem.Domain.Shared.Results;

namespace Phloem.Application.Features.Orders.Queries;

/// <summary>Handles <see cref="GetBuyAgainQuery"/>.</summary>
public sealed class GetBuyAgainQueryHandler(
    ICurrentUser currentUser,
    IOrderRepository orders,
    IDishRepository dishes)
    : IRequestHandler<GetBuyAgainQuery, Result<BuyAgainDto>>
{
    /// <summary>Splits the old order's dishes into "can order" (with quantities) and "can't" (names).</summary>
    /// <returns>The basket items; or <see cref="OrderErrors.NotFound"/> if it isn't the customer's order.</returns>
    /// <exception cref="InvalidOperationException">Called without a signed-in user (endpoint misconfigured).</exception>
    public async Task<Result<BuyAgainDto>> Handle(GetBuyAgainQuery query, CancellationToken ct)
    {
        var userId = currentUser.UserId
            ?? throw new InvalidOperationException("This action requires a signed-in user.");

        var order = await orders.GetByOrderNumberAsync(query.OrderNumber.Trim(), ct);
        if (order is null || order.CustomerId != userId) return OrderErrors.NotFound;

        var current = (await dishes.GetByIdsAsync(order.Lines.Select(l => l.DishId).ToList(), ct))
                      .ToDictionary(d => d.Id);

        var items = new List<BasketItemDto>();
        var unavailable = new List<string>();

        foreach (var line in order.Lines)
        {
            if (current.TryGetValue(line.DishId, out var dish) && dish.CanBeOrdered)
                items.Add(new BasketItemDto(line.DishId, line.Quantity));
            else
                unavailable.Add(line.DishName);
        }

        return new BuyAgainDto(items, unavailable);
    }
}