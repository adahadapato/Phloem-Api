using Microsoft.EntityFrameworkCore;
using Phloem.Domain.Entities;
using Phloem.Domain.Enums;
using Phloem.Domain.Interfaces;
using Phloem.Domain.Shared.Results;

namespace Phloem.Persistence.Repositories;

/// <summary><see cref="IStaffAuditRepository"/> with EF Core: add, and read pages for the owners' audit page.</summary>
internal sealed class StaffAuditRepository(PhloemDbContext context)
    : Repository<StaffAuditEntry>(context), IStaffAuditRepository
{
    /// <inheritdoc />
    public Task<PagedResult<StaffAuditEntry>> GetPagedAsync(Guid? actorId, StaffAction? action, Country? country,
                                                            DateTime? fromUtc, DateTime? toUtc, PageRequest page,
                                                            CancellationToken cancellationToken = default)
    {
        IQueryable<StaffAuditEntry> query = Set.AsNoTracking();

        if (actorId is not null) query = query.Where(a => a.ActorId == actorId);
        if (action is not null) query = query.Where(a => a.Action == action);
        if (country is not null) query = query.Where(a => a.Country == country);
        if (fromUtc is not null) query = query.Where(a => a.CreatedAt >= fromUtc);
        if (toUtc is not null) query = query.Where(a => a.CreatedAt < toUtc);

        return ToPagedAsync(query.OrderByDescending(a => a.CreatedAt), page, cancellationToken);
    }
}
