using Phloem.Domain.Entities;
using Phloem.Domain.Enums;
using Phloem.Domain.Shared.Results;

namespace Phloem.Domain.Interfaces;

/// <summary>Changes waiting for (or decided by) a second person.</summary>
public interface IPendingChangeRepository : IRepository<PendingChange>
{
    /// <summary>Every change still waiting (read-only), oldest first; the Application layer filters what each person
    /// may decide. Waiting changes are few, so they're read in one go.</summary>
    /// <param name="cancellationToken">Stops the query.</param>
    Task<IReadOnlyList<PendingChange>> GetWaitingAsync(CancellationToken cancellationToken = default);

    /// <summary>The waiting change of this kind for this record (tracked), if any.</summary>
    /// <param name="kind">The kind.</param>
    /// <param name="targetId">The record (e.g. the shop); null for Phloem-wide changes of that kind.</param>
    /// <param name="cancellationToken">Stops the query.</param>
    Task<PendingChange?> GetWaitingForAsync(PendingChangeKind kind, Guid? targetId,
                                            CancellationToken cancellationToken = default);

    /// <summary>Waiting changes of this kind (tracked), e.g. every waiting Terms proposal.</summary>
    /// <param name="kind">The kind.</param>
    /// <param name="cancellationToken">Stops the query.</param>
    Task<IReadOnlyList<PendingChange>> GetWaitingOfKindAsync(PendingChangeKind kind,
                                                             CancellationToken cancellationToken = default);

    /// <summary>Waiting changes whose time is up (tracked), for the expiry task.</summary>
    /// <param name="utcNow">The current time.</param>
    /// <param name="cancellationToken">Stops the query.</param>
    Task<IReadOnlyList<PendingChange>> GetExpiredAsync(DateTime utcNow, CancellationToken cancellationToken = default);

    /// <summary>One page of decided changes (read-only), newest first.</summary>
    /// <param name="proposedById">Only changes this person proposed; null = everyone's.</param>
    /// <param name="coveredCountries">Only these countries (Phloem-wide changes included); null = every country.</param>
    /// <param name="includeWaiting">True to include changes still waiting (for "my proposals").</param>
    /// <param name="page">Which page and how many per page.</param>
    /// <param name="cancellationToken">Stops the query.</param>
    Task<PagedResult<PendingChange>> GetHistoryAsync(Guid? proposedById, IReadOnlyCollection<Country>? coveredCountries,
                                                     bool includeWaiting, PageRequest page,
                                                     CancellationToken cancellationToken = default);
}
