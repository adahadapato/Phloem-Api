using Phloem.Domain.Entities;
using Phloem.Domain.Enums;
using Phloem.Domain.Models;
using Phloem.Domain.Shared.Results;

namespace Phloem.Domain.Interfaces;

/// <summary>The AI call log: added by the AI client, read on Admin → AI, deleted after the keeping period.</summary>
public interface IAiCallLogRepository : IRepository<AiCallLog>
{
    /// <summary>Calls, tokens and cost since a moment (e.g. the start of this month).</summary>
    /// <param name="fromUtc">The start.</param>
    /// <param name="cancellationToken">Stops the query.</param>
    Task<AiUsage> GetUsageSinceAsync(DateTime fromUtc, CancellationToken cancellationToken = default);

    /// <summary>One page of calls, newest first, without their text (read-only).</summary>
    /// <param name="promptKey">Only this job; null = all.</param>
    /// <param name="outcome">Only calls that ended like this; null = all.</param>
    /// <param name="page">Which page.</param>
    /// <param name="cancellationToken">Stops the query.</param>
    Task<PagedResult<AiCallLog>> GetPagedAsync(string? promptKey, AiCallOutcome? outcome, PageRequest page,
                                               CancellationToken cancellationToken = default);

    /// <summary>Deletes (for good) every call older than a moment.</summary>
    /// <param name="beforeUtc">Calls made before this are deleted.</param>
    /// <param name="cancellationToken">Stops the delete.</param>
    /// <returns>How many were deleted.</returns>
    Task<int> DeleteOlderThanAsync(DateTime beforeUtc, CancellationToken cancellationToken = default);
}
