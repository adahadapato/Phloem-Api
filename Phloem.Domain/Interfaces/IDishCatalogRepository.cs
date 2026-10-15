using Phloem.Domain.Entities;

namespace Phloem.Domain.Interfaces;

public interface IDishCatalogRepository : IRepository<DishCatalog>
{
    /// <summary>
    /// Add / Edit dish → the dish-name picker: every Approved name, ordered by group then name.
    /// </summary>
    Task<IReadOnlyList<DishCatalog>> GetApprovedAsync(CancellationToken cancellationToken = default);

    /// <summary>Admin dashboard → "Dish name suggestions": names waiting for review, oldest first.</summary>
    Task<IReadOnlyList<DishCatalog>> GetSuggestionsAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// "Can't find your dish?": stops a duplicate of an Approved or already-Suggested name
    /// (case-insensitive). Rejected names don't count, so they can be suggested again.
    /// </summary>
    Task<bool> NameExistsAsync(string name, CancellationToken cancellationToken = default);
}