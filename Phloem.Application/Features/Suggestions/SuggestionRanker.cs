using Phloem.Domain.Entities;
using Phloem.Domain.Enums;
using Phloem.Domain.Models;

namespace Phloem.Application.Features.Suggestions;

/// <summary>A dish picked for "For you", with its score and the one reason shown to the customer.</summary>
/// <param name="Dish">The dish (loaded with its shop, photos and dish-list name).</param>
/// <param name="Score">Higher = shown first.</param>
/// <param name="Reason">e.g. "Often ordered with Jollof Rice".</param>
public sealed record RankedDish(Dish Dish, double Score, string Reason);

/// <summary>
/// The "For you" scoring, kept apart from the database so it is easy to read, test and tune, and so a learned
/// (AI) ranker can replace it later behind the same query.
/// </summary>
/// <remarks>
/// <para><b>Signals</b> (each turned into points; the biggest one gives the reason shown on the card):</para>
/// <list type="bullet">
///   <item><description><b>Often ordered with</b> a dish they bought (other customers' orders): up to 4.</description></item>
///   <item><description><b>A shop they've ordered from</b>: up to 2, by how much of their ordering is there.</description></item>
///   <item><description><b>Their usual cuisine</b> (Ghanaian, Nigerian…): up to 2; <b>type of food</b> (meal, snack, drink): up to 1.</description></item>
///   <item><description><b>Popular in the country</b> (last 90 days): up to 1.5.</description></item>
///   <item><description><b>Well rated</b>: up to 0.5, only with real ratings.</description></item>
/// </list>
/// <para><b>Rules:</b> dishes already bought are left out (they're in "Order again"); at most
/// <see cref="MaxPerShop"/> from one shop, so one big menu can't fill the row.</para>
/// </remarks>
public static class SuggestionRanker
{
    /// <summary>Most suggestions from one shop.</summary>
    public const int MaxPerShop = 2;

    /// <summary>
    /// Scores every candidate and returns the best <paramref name="take"/>, each with its reason.
    /// </summary>
    /// <param name="candidates">Dishes that can be ordered now in the country.</param>
    /// <param name="purchased">What the customer has bought (any country), most recent first. Empty = new customer.</param>
    /// <param name="pairs">"Ordered X → also ordered Y" counts for the customer's dishes.</param>
    /// <param name="popularDishIds">Most ordered dishes in the country, most ordered first.</param>
    /// <param name="countryName">e.g. "Ghana", for "Popular in Ghana".</param>
    /// <param name="take">How many to return.</param>
    public static IReadOnlyList<RankedDish> Rank(
        IReadOnlyList<Dish> candidates,
        IReadOnlyList<PurchasedDish> purchased,
        IReadOnlyList<DishPairCount> pairs,
        IReadOnlyList<Guid> popularDishIds,
        string countryName,
        int take)
    {
        var bought = purchased.Select(p => p.DishId).ToHashSet();
        var boughtNames = purchased.ToDictionary(p => p.DishId, p => p.DishName);
        var totalOrders = Math.Max(1, purchased.Sum(p => p.Orders));

        // How much of their ordering is in each shop / cuisine / type of food (0–1)
        var shopShare = Shares(purchased, p => p.VendorId, totalOrders);
        var groupShare = Shares(purchased, p => p.Group, totalOrders);
        var categoryShare = Shares(purchased, p => p.Category, totalOrders);
        var shopNames = purchased.GroupBy(p => p.VendorId).ToDictionary(g => g.Key, g => g.First().VendorName);

        // Best pair per suggested dish ("Often ordered with …")
        var bestPair = pairs.GroupBy(p => p.OtherDishId)
                            .ToDictionary(g => g.Key, g => g.MaxBy(p => p.Orders)!);
        var maxPair = Math.Max(1, pairs.Count == 0 ? 1 : pairs.Max(p => p.Orders));

        // Popularity rank → 1.0 for the most ordered, down to ~0 for the last
        var popularRank = popularDishIds.Select((id, i) => (id, i))
                                        .ToDictionary(x => x.id, x => 1.0 - (double)x.i / Math.Max(1, popularDishIds.Count));

        var ranked = new List<RankedDish>();
        foreach (var dish in candidates)
        {
            if (bought.Contains(dish.Id)) continue;

            var signals = new List<(double Points, string Reason)>();

            if (bestPair.TryGetValue(dish.Id, out var pair) && boughtNames.TryGetValue(pair.BoughtDishId, out var withName))
                signals.Add((4.0 * pair.Orders / maxPair, $"Often ordered with {withName}"));

            if (shopShare.TryGetValue(dish.VendorId, out var shop))
                signals.Add((2.0 * shop, $"From {shopNames[dish.VendorId]}, a shop you've ordered from"));

            var group = dish.DishCatalog?.Group;
            if (group is { } g && groupShare.TryGetValue(g, out var cuisine))
                signals.Add((2.0 * cuisine, $"More {GroupText(g)} you might like"));

            if (categoryShare.TryGetValue(dish.Category, out var kind))
                signals.Add((1.0 * kind, $"More {CategoryText(dish.Category)} you might like"));

            if (popularRank.TryGetValue(dish.Id, out var popular))
                signals.Add((1.5 * popular, $"Popular in {countryName}"));

            if (dish.RatingCount > 0)
                signals.Add((0.5 * (double)dish.Rating / 5.0, $"Top rated in {countryName}"));

            if (signals.Count == 0)
            {
                // Nothing in common: still worth showing to fill the row, last
                ranked.Add(new RankedDish(dish, 0, $"New to try in {countryName}"));
                continue;
            }

            var reason = signals.MaxBy(s => s.Points).Reason;
            ranked.Add(new RankedDish(dish, signals.Sum(s => s.Points), reason));
        }

        // Best first, then at most MaxPerShop from one shop
        var perShop = new Dictionary<Guid, int>();
        var result = new List<RankedDish>(take);
        foreach (var r in ranked.OrderByDescending(r => r.Score)
                                .ThenByDescending(r => r.Dish.Rating)
                                .ThenBy(r => r.Dish.Name))
        {
            var count = perShop.GetValueOrDefault(r.Dish.VendorId);
            if (count >= MaxPerShop) continue;
            perShop[r.Dish.VendorId] = count + 1;
            result.Add(r);
            if (result.Count == take) break;
        }

        return result;
    }

    /// <summary>"Ghanaian dishes", "snacks"…</summary>
    public static string GroupText(CatalogGroup group) => group switch
    {
        CatalogGroup.Ghana => "Ghanaian dishes",
        CatalogGroup.Nigeria => "Nigerian dishes",
        CatalogGroup.WestAfrican => "West African dishes",
        CatalogGroup.Snacks => "snacks",
        CatalogGroup.Drinks => "drinks",
        _ => "dishes"
    };

    /// <summary>"local meals", "snacks", "drinks".</summary>
    public static string CategoryText(DishCategory category) => category switch
    {
        DishCategory.LocalMeals => "local meals",
        DishCategory.Snacks => "snacks",
        DishCategory.Drinks => "drinks",
        _ => "dishes"
    };

    private static Dictionary<TKey, double> Shares<TKey>(IReadOnlyList<PurchasedDish> purchased,
                                                         Func<PurchasedDish, TKey> key, int totalOrders)
        where TKey : notnull =>
        purchased.GroupBy(key).ToDictionary(g => g.Key, g => (double)g.Sum(p => p.Orders) / totalOrders);
}
