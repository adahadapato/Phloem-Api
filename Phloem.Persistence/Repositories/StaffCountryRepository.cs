using Microsoft.EntityFrameworkCore;
using Phloem.Domain.Entities;
using Phloem.Domain.Interfaces;

namespace Phloem.Persistence.Repositories;

/// <summary><see cref="IStaffCountryRepository"/> with EF Core.</summary>
internal sealed class StaffCountryRepository(PhloemDbContext context)
    : Repository<StaffCountry>(context), IStaffCountryRepository
{
    /// <inheritdoc />
    public async Task<IReadOnlyList<StaffCountry>> GetForUserAsync(Guid userId,
                                                                   CancellationToken cancellationToken = default) =>
        await Set.AsNoTracking().Where(s => s.UserId == userId).ToListAsync(cancellationToken);

    /// <inheritdoc />
    public async Task<IReadOnlyList<StaffCountry>> GetForUserToChangeAsync(
        Guid userId, CancellationToken cancellationToken = default) =>
        await Set.Where(s => s.UserId == userId).ToListAsync(cancellationToken);

    /// <inheritdoc />
    public Task<bool> HasAnyAsync(Guid userId, CancellationToken cancellationToken = default) =>
        Set.AnyAsync(s => s.UserId == userId, cancellationToken);
}
