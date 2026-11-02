using Phloem.Domain.Entities;
using Phloem.Domain.Enums;
using Phloem.Domain.Shared.Results;

namespace Phloem.Domain.Interfaces;

/// <summary>
/// The staff audit log. Lines are only ever added (<see cref="IRepository{TEntity}.Add"/>) and read on the owners'
/// audit page; never changed or deleted.
/// </summary>
public interface IStaffAuditRepository : IRepository<StaffAuditEntry>
{
    /// <summary>One page of the log, newest first (read-only).</summary>
    /// <param name="actorId">Only what this person did; null = everyone.</param>
    /// <param name="action">Only this action; null = all.</param>
    /// <param name="country">Only this country; null = all (lines without a country included).</param>
    /// <param name="fromUtc">Only from this moment; null = from the start.</param>
    /// <param name="toUtc">Only before this moment; null = up to now.</param>
    /// <param name="page">Which page and how many per page.</param>
    /// <param name="cancellationToken">Stops the query.</param>
    /// <returns>The page.</returns>
    Task<PagedResult<StaffAuditEntry>> GetPagedAsync(Guid? actorId, StaffAction? action, Country? country,
                                                     DateTime? fromUtc, DateTime? toUtc, PageRequest page,
                                                     CancellationToken cancellationToken = default);
}
