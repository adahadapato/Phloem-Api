using MediatR;
using Phloem.Application.Features.Dishes.Dtos;
using Phloem.Application.Features.Suggestions.Dtos;
using Phloem.Domain.Enums;
using Phloem.Domain.Interfaces;
using Phloem.Domain.Models;
using Phloem.Domain.Shared.Results;

namespace Phloem.Application.Features.Suggestions.Queries;

/// <summary>Handles <see cref="GetMySuggestionsQuery"/>.</summary>
public sealed class GetMySuggestionsQueryHandler(
    ICurrentUser currentUser,
    IOrderRepository orders,
    IDishRepository dishes,
    IFileStorage storage,
    TimeProvider clock)
    : IRequestHandler<GetMySuggestionsQuery, Result<SuggestionsDto>>
{
    /// <summary>How many of the customer's newest orders say what they like.</summary>
    private const int HistoryOrders = 50;

    /// <summary>How many of their most recent dishes "also ordered" looks at.</summary>
    private const int PairSourceDishes = 30;

    /// <summary>Most dishes scored at once (best rated first); plenty for one country today.</summary>
    internal const int MaxCandidates = 400;

    /// <summary>
    /// Four small reads (their history, the dishes on sale, what goes together, what's popular), then scoring in
    /// memory by <see cref="SuggestionRanker"/>.
    /// </summary>
    /// <exception cref="InvalidOperationException">Called without a signed-in user (endpoint misconfigured).</exception>
    public async Task<Result<SuggestionsDto>> Handle(GetMySuggestionsQuery query, CancellationToken ct)
    {
        var userId = currentUser.UserId
            ?? throw new InvalidOperationException("This action requires a signed-in user.");
        var now = clock.GetUtcNow().UtcDateTime;

        var purchased = await orders.GetPurchasedDishesAsync(userId, HistoryOrders, ct);
        var candidates = await dishes.GetOrderableInCountryAsync(query.Country, MaxCandidates, ct);
        var popular = await orders.GetMostOrderedDishIdsAsync(query.Country, now.AddDays(-90), 40, ct);
        IReadOnlyList<DishPairCount> pairs = purchased.Count == 0
            ? []
            : await orders.GetAlsoOrderedAsync(purchased.Take(PairSourceDishes).Select(p => p.DishId).ToList(),
                                               query.Country, now.AddDays(-180), 200, ct);

        // Order again: what they bought, newest first, if it's on sale here now
        var onSale = candidates.ToDictionary(d => d.Id);
        IReadOnlyList<DishCardDto> orderAgain = purchased
            .Where(p => onSale.ContainsKey(p.DishId))
            .Take(query.Take)
            .Select(p => DishCardDto.From(onSale[p.DishId], storage))
            .ToList();

        // For you: new dishes, scored
        IReadOnlyList<SuggestedDishDto> forYou = SuggestionRanker
            .Rank(candidates, purchased, pairs, popular, query.Country.DisplayName(), query.Take)
            .Select(r => new SuggestedDishDto(DishCardDto.From(r.Dish, storage), r.Reason))
            .ToList();

        return Result.Success(new SuggestionsDto(orderAgain, forYou, IsPersonal: purchased.Count > 0));
    }
}
