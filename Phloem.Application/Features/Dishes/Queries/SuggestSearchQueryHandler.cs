using MediatR;
using Phloem.Application.Features.Dishes.Dtos;
using Phloem.Domain.Interfaces;
using Phloem.Domain.Models;
using Phloem.Domain.Shared.Results;

namespace Phloem.Application.Features.Dishes.Queries;

/// <summary>Handles <see cref="SuggestSearchQuery"/>.</summary>
public sealed class SuggestSearchQueryHandler(IDishRepository dishes, IVendorRepository vendors)
    : IRequestHandler<SuggestSearchQuery, Result<IReadOnlyList<SearchSuggestionDto>>>
{
    /// <summary>How many matching names are read before ranking (plenty for one word).</summary>
    private const int Candidates = 40;

    /// <summary>
    /// Reads matching dish names (most dishes first) and shops, then ranks the names: those that START with the text
    /// first ("jol" → "Jollof…"), then those with a WORD starting with it ("ric" → "Jollof Rice"), then the rest;
    /// more dishes and then shorter names first within each group.
    /// </summary>
    public async Task<Result<IReadOnlyList<SearchSuggestionDto>>> Handle(SuggestSearchQuery query, CancellationToken ct)
    {
        var text = query.Q.Trim();

        var names = await dishes.GetNameMatchesAsync(query.Country, text, Candidates, ct);
        var shops = await vendors.GetPagedAsync(null, query.Country, text,
            new PageRequest { Page = 1, PageSize = SuggestSearchQuery.MaxShops }, ct, visibleToBuyersOnly: true);

        var result = Rank(names, text)
            .Take(query.Take)
            .Select(n => new SearchSuggestionDto(n.Name, SearchSuggestionKind.Dish, n.DishCount, null))
            .Concat(shops.Items.Select(v => new SearchSuggestionDto(v.Name, SearchSuggestionKind.Shop, 0, v.Id)))
            .ToList();

        return Result.Success<IReadOnlyList<SearchSuggestionDto>>(result);
    }

    /// <summary>Best matches first (see <see cref="Handle"/>). Public for tests.</summary>
    public static IEnumerable<DishNameMatch> Rank(IEnumerable<DishNameMatch> names, string text) =>
        names.OrderBy(n => MatchRank(n.Name, text))
             .ThenByDescending(n => n.DishCount)
             .ThenBy(n => n.Name.Length)
             .ThenBy(n => n.Name, StringComparer.OrdinalIgnoreCase);

    /// <summary>0 = the name starts with it, 1 = a word starts with it, 2 = it's somewhere inside.</summary>
    private static int MatchRank(string name, string text)
    {
        if (name.StartsWith(text, StringComparison.OrdinalIgnoreCase)) return 0;
        var words = name.Split([' ', '-', '&', '(', ')', '/', ','], StringSplitOptions.RemoveEmptyEntries);
        return words.Any(w => w.StartsWith(text, StringComparison.OrdinalIgnoreCase)) ? 1 : 2;
    }
}
