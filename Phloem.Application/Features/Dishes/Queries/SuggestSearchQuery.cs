using MediatR;
using Phloem.Application.Common.Caching;
using Phloem.Application.Features.Dishes.Dtos;
using Phloem.Domain.Enums;
using Phloem.Domain.Shared.Results;

namespace Phloem.Application.Features.Dishes.Queries;

/// <summary>
/// Search predictions while someone types (Home, Explore, the app): full dish names containing the text (up to
/// <paramref name="Take"/>), then up to <see cref="MaxShops"/> shops whose name contains it. Only what can be ordered
/// now. The same for everyone, so cached briefly (Dishes and Vendors tags: cleared whenever dishes or shops change).
/// </summary>
/// <param name="Q">What has been typed (2–50 characters).</param>
/// <param name="Country">The country shown; null = every country.</param>
/// <param name="Take">How many dish names at most (1–10).</param>
[Cacheable(CacheTags.Dishes, CacheTags.Vendors, Minutes = 5)]
public sealed record SuggestSearchQuery(string Q, Country? Country = null, int Take = SuggestSearchQuery.DefaultTake)
    : IRequest<Result<IReadOnlyList<SearchSuggestionDto>>>
{
    /// <summary>Dish names when the caller doesn't say.</summary>
    public const int DefaultTake = 6;

    /// <summary>Most dish names.</summary>
    public const int MaxTake = 10;

    /// <summary>Shops shown after the dish names.</summary>
    public const int MaxShops = 3;

    /// <summary>Fewest characters before predictions start.</summary>
    public const int MinLength = 2;

    /// <summary>Longest text accepted.</summary>
    public const int MaxLength = 50;
}
