using Phloem.Domain.Entities;

namespace Phloem.Domain.Interfaces;

/// <summary>Loads and saves the "what can be sent" list (<see cref="ParcelItemRule"/>). Implemented in Persistence.</summary>
public interface IParcelItemRuleRepository : IRepository<ParcelItemRule>
{
    /// <summary>The whole list in display order (read-only).</summary>
    /// <param name="cancellationToken">Cancels the database call.</param>
    Task<IReadOnlyList<ParcelItemRule>> GetAllAsync(CancellationToken cancellationToken = default);

    /// <summary>The whole list, tracked, for reordering.</summary>
    /// <param name="cancellationToken">Cancels the database call.</param>
    Task<IReadOnlyList<ParcelItemRule>> GetAllForUpdateAsync(CancellationToken cancellationToken = default);

    /// <summary>The highest position in use; -1 when the list is empty (so the next line goes at the end).</summary>
    /// <param name="cancellationToken">Cancels the database call.</param>
    Task<int> GetLastSortOrderAsync(CancellationToken cancellationToken = default);
}
