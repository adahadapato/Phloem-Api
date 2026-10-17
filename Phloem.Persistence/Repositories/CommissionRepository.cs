using Microsoft.EntityFrameworkCore;
using Phloem.Domain.Entities;
using Phloem.Domain.Enums;
using Phloem.Domain.Interfaces;

namespace Phloem.Persistence.Repositories;

/// <summary>EF Core implementation of <see cref="ICommissionRepository"/>.</summary>
internal sealed class CommissionRepository(PhloemDbContext context)
    : Repository<Commission>(context), ICommissionRepository
{
    /// <inheritdoc />
    /// <remarks>Tracked: admins change it through this. Null = no rate set (0%).</remarks>
    public Task<Commission?> GetByCountryAsync(Country country, CancellationToken cancellationToken = default) =>
        Set.FirstOrDefaultAsync(c => c.Country == country, cancellationToken);

    /// <inheritdoc />
    public async Task<IReadOnlyList<Commission>> GetAllAsync(CancellationToken cancellationToken = default) =>
        await Set.AsNoTracking().OrderBy(c => c.Country).ToListAsync(cancellationToken);
}