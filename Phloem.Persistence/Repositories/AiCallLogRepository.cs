using Microsoft.EntityFrameworkCore;
using Phloem.Domain.Entities;
using Phloem.Domain.Enums;
using Phloem.Domain.Interfaces;
using Phloem.Domain.Models;
using Phloem.Domain.Shared.Results;

namespace Phloem.Persistence.Repositories;

/// <summary><see cref="IAiCallLogRepository"/> with EF Core.</summary>
internal sealed class AiCallLogRepository(PhloemDbContext context)
    : Repository<AiCallLog>(context), IAiCallLogRepository
{
    /// <inheritdoc />
    public async Task<AiUsage> GetUsageSinceAsync(DateTime fromUtc, CancellationToken cancellationToken = default)
    {
        var usage = await Set.AsNoTracking()
            .Where(c => c.CreatedAt >= fromUtc)
            .GroupBy(_ => 1)
            .Select(g => new
            {
                Calls = g.Count(),
                Failed = g.Count(c => c.Outcome != AiCallOutcome.Succeeded),
                In = g.Sum(c => (long)c.InputTokens),
                Out = g.Sum(c => (long)c.OutputTokens),
                Cost = g.Sum(c => c.CostUsd)
            })
            .FirstOrDefaultAsync(cancellationToken);

        return usage is null ? AiUsage.None : new AiUsage(usage.Calls, usage.Failed, usage.In, usage.Out, usage.Cost);
    }

    /// <inheritdoc />
    public Task<PagedResult<AiCallLog>> GetPagedAsync(string? promptKey, AiCallOutcome? outcome, PageRequest page,
                                                      CancellationToken cancellationToken = default)
    {
        IQueryable<AiCallLog> query = Set.AsNoTracking();
        if (!string.IsNullOrWhiteSpace(promptKey)) query = query.Where(c => c.PromptKey == promptKey);
        if (outcome is { } o) query = query.Where(c => c.Outcome == o);

        return ToPagedAsync(query.OrderByDescending(c => c.CreatedAt), page, cancellationToken);
    }

    /// <inheritdoc />
    /// <remarks>A real delete (not the soft delete): the point is that people's text is gone.</remarks>
    public Task<int> DeleteOlderThanAsync(DateTime beforeUtc, CancellationToken cancellationToken = default) =>
        Set.IgnoreQueryFilters().Where(c => c.CreatedAt < beforeUtc).ExecuteDeleteAsync(cancellationToken);
}
