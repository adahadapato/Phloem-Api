namespace Phloem.Domain.Models;

/// <summary>
/// A dish name that matches what someone is typing in a search box, and how many dishes that can be ordered now carry
/// it (e.g. "Jollof Rice", 7). Read by <c>IDishRepository.GetNameMatchesAsync</c> for search predictions.
/// </summary>
/// <param name="Name">The dish's full name as buyers see it.</param>
/// <param name="DishCount">How many orderable dishes have exactly this name (more = more choice).</param>
public sealed record DishNameMatch(string Name, int DishCount);
