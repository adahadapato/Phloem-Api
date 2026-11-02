using Microsoft.EntityFrameworkCore;
using Phloem.Domain.Entities;
using Phloem.Domain.Enums;
using Phloem.Domain.Interfaces;
using Phloem.Domain.Shared.Results;

namespace Phloem.Persistence.Repositories;

/// <summary><see cref="IPendingChangeRepository"/> with EF Core.</summary>
internal sealed class PendingChangeRepository(PhloemDbContext context)
    : Repository<PendingChange>(context), IPendingChangeRepository
{
    /// <inheritdoc />
    public async Task<IReadOnlyList<PendingChange>> GetWaitingAsync(CancellationToken cancellationToken = default) =>
        await Set.AsNoTracking()
                 .Where(p => p.Status == PendingChangeStatus.Waiting)
                 .OrderBy(p => p.CreatedAt)
                 .ToListAsync(cancellationToken);

    /// <inheritdoc />
    public Task<PendingChange?> GetWaitingForAsync(PendingChangeKind kind, Guid? targetId,
                                                   CancellationToken cancellationToken = default) =>
        Set.Where(p => p.Status == PendingChangeStatus.Waiting && p.Kind == kind && p.TargetId == targetId)
           .OrderByDescending(p => p.CreatedAt)
           .FirstOrDefaultAsync(cancellationToken);

    /// <inheritdoc />
    public async Task<IReadOnlyList<PendingChange>> GetWaitingOfKindAsync(PendingChangeKind kind,
                                                                          CancellationToken cancellationToken = default) =>
        await Set.Where(p => p.Status == PendingChangeStatus.Waiting && p.Kind == kind)
                 .ToListAsync(cancellationToken);

    /// <inheritdoc />
    public async Task<IReadOnlyList<PendingChange>> GetExpiredAsync(DateTime utcNow,
                                                                    CancellationToken cancellationToken = default) =>
        await Set.Where(p => p.Status == PendingChangeStatus.Waiting && p.ExpiresAtUtc <= utcNow)
                 .ToListAsync(cancellationToken);

    /// <inheritdoc />
    public Task<PagedResult<PendingChange>> GetHistoryAsync(Guid? proposedById,
                                                            IReadOnlyCollection<Country>? coveredCountries,
                                                            bool includeWaiting, PageRequest page,
                                                            CancellationToken cancellationToken = default)
    {
        IQueryable<PendingChange> query = Set.AsNoTracking();

        if (!includeWaiting) query = query.Where(p => p.Status != PendingChangeStatus.Waiting);
        if (proposedById is not null) query = query.Where(p => p.ProposedById == proposedById);
        if (coveredCountries is not null)
        {
            var covered = coveredCountries.Cast<Country?>().ToList();
            query = query.Where(p => p.Country == null || covered.Contains(p.Country));
        }

        return ToPagedAsync(query.OrderByDescending(p => p.CreatedAt), page, cancellationToken);
    }
}
