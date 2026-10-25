using Microsoft.EntityFrameworkCore;
using Phloem.Domain.Entities;
using Phloem.Domain.Enums;
using Phloem.Domain.Interfaces;

namespace Phloem.Persistence.Repositories;

/// <summary>EF Core implementation of <see cref="ILegalDocumentRepository"/>.</summary>
internal sealed class LegalDocumentRepository(PhloemDbContext context)
    : Repository<LegalDocument>(context), ILegalDocumentRepository
{
    /// <inheritdoc />
    /// <remarks>Read-only.</remarks>
    public Task<LegalDocument?> GetCurrentAsync(LegalDocumentType type,
                                                CancellationToken cancellationToken = default) =>
        Set.AsNoTracking()
           .Where(d => d.Type == type)
           .OrderByDescending(d => d.Version)
           .FirstOrDefaultAsync(cancellationToken);

    /// <inheritdoc />
    /// <remarks>Deleted rows count too, so a number is never reused.</remarks>
    public async Task<int> GetLatestVersionAsync(LegalDocumentType type,
                                                 CancellationToken cancellationToken = default) =>
        await Set.IgnoreQueryFilters()
                 .Where(d => d.Type == type)
                 .MaxAsync(d => (int?)d.Version, cancellationToken) ?? 0;
}
