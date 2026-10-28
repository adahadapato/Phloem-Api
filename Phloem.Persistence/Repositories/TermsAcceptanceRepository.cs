using Microsoft.EntityFrameworkCore;
using Phloem.Domain.Entities;
using Phloem.Domain.Enums;
using Phloem.Domain.Interfaces;

namespace Phloem.Persistence.Repositories;

/// <summary>EF Core implementation of <see cref="ITermsAcceptanceRepository"/>. All reads are read-only.</summary>
internal sealed class TermsAcceptanceRepository(PhloemDbContext context)
    : Repository<TermsAcceptance>(context), ITermsAcceptanceRepository
{
    /// <inheritdoc />
    public Task<int?> GetLatestVersionAsync(Guid userId, LegalDocumentType documentType,
                                            CancellationToken cancellationToken = default) =>
        Set.AsNoTracking()
           .Where(a => a.UserId == userId && a.DocumentType == documentType)
           .MaxAsync(a => (int?)a.Version, cancellationToken);

    /// <inheritdoc />
    public async Task<IReadOnlyDictionary<Guid, (int Version, DateTime AcceptedAt)>> GetLatestTermsAsync(
        IReadOnlyCollection<Guid> userIds, CancellationToken cancellationToken = default)
    {
        if (userIds.Count == 0) return new Dictionary<Guid, (int, DateTime)>();
        var ids = userIds.ToList();

        var rows = await Set.AsNoTracking()
            .Where(a => ids.Contains(a.UserId) && a.DocumentType == LegalDocumentType.Terms)
            .Select(a => new { a.UserId, a.Version, a.CreatedAt })
            .ToListAsync(cancellationToken);

        // Latest version per person, and the first time they accepted it
        return rows.GroupBy(r => r.UserId)
                   .ToDictionary(g => g.Key, g =>
                   {
                       var version = g.Max(r => r.Version);
                       return (version, g.Where(r => r.Version == version).Min(r => r.CreatedAt));
                   });
    }

    /// <inheritdoc />
    public Task<int> CountPeopleAtLeastAsync(LegalDocumentType documentType, int version,
                                             CancellationToken cancellationToken = default) =>
        Set.AsNoTracking()
           .Where(a => a.DocumentType == documentType && a.Version >= version)
           .Select(a => a.UserId)
           .Distinct()
           .CountAsync(cancellationToken);
}
