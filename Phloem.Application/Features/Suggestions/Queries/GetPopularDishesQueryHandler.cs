using MediatR;
using Phloem.Application.Features.Dishes.Dtos;
using Phloem.Application.Features.Suggestions.Dtos;
using Phloem.Domain.Enums;
using Phloem.Domain.Interfaces;
using Phloem.Domain.Shared.Results;

namespace Phloem.Application.Features.Suggestions.Queries;

/// <summary>Handles <see cref="GetPopularDishesQuery"/>: the ranker with no history (popularity and ratings only).</summary>
public sealed class GetPopularDishesQueryHandler(
    IOrderRepository orders,
    IDishRepository dishes,
    IFileStorage storage,
    TimeProvider clock)
    : IRequestHandler<GetPopularDishesQuery, Result<IReadOnlyList<SuggestedDishDto>>>
{
    public async Task<Result<IReadOnlyList<SuggestedDishDto>>> Handle(GetPopularDishesQuery query,
                                                                      CancellationToken ct)
    {
        var now = clock.GetUtcNow().UtcDateTime;
        var candidates = await dishes.GetOrderableInCountryAsync(
            query.Country, GetMySuggestionsQueryHandler.MaxCandidates, ct);
        var popular = await orders.GetMostOrderedDishIdsAsync(query.Country, now.AddDays(-90), 40, ct);

        IReadOnlyList<SuggestedDishDto> result = SuggestionRanker
            .Rank(candidates, [], [], popular, query.Country.DisplayName(), query.Take)
            .Select(r => new SuggestedDishDto(DishCardDto.From(r.Dish, storage), r.Reason))
            .ToList();

        return Result.Success(result);
    }
}
