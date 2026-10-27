namespace Phloem.Domain.Models;

/// <summary>
/// "People who ordered X also ordered Y": how many orders had both dishes. Counted from every customer's paid
/// orders by <c>IOrderRepository.GetAlsoOrderedAsync</c>.
/// </summary>
/// <param name="BoughtDishId">A dish the customer has bought (X).</param>
/// <param name="OtherDishId">A dish in the same orders that they haven't bought (Y).</param>
/// <param name="Orders">How many orders had both.</param>
public sealed record DishPairCount(Guid BoughtDishId, Guid OtherDishId, int Orders);
