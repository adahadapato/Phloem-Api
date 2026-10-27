using MediatR;
using Phloem.Application.Common.Caching;
using Phloem.Application.Features.Suggestions.Dtos;
using Phloem.Domain.Enums;
using Phloem.Domain.Shared.Results;

namespace Phloem.Application.Features.Suggestions.Queries;

/// <summary>
/// Home when nobody is signed in: "Popular in Ghana" (most ordered in the last 90 days, then best rated), at most
/// two per shop, each with its reason. The same for everyone, so cached; cleared whenever dishes change.
/// </summary>
/// <param name="Country">The country Home is showing.</param>
/// <param name="Take">How many dishes (1–20).</param>
[Cacheable(CacheTags.Dishes, Minutes = 30)]
public sealed record GetPopularDishesQuery(Country Country, int Take = GetMySuggestionsQuery.DefaultTake)
    : IRequest<Result<IReadOnlyList<SuggestedDishDto>>>;
