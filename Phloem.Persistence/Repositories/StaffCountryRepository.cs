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
    public async Task<IReadOnlyDictionary<Guid, IReadOnlyList<StaffCountry>>> GetForUsersAsync(
        IReadOnlyCollection<Guid> userIds, CancellationToken cancellationToken = default)
    {
        if (userIds.Count == 0) return new Dictionary<Guid, IReadOnlyList<StaffCountry>>();
        var ids = userIds.ToList();

        var rows = await Set.AsNoTracking().Where(s => ids.Contains(s.UserId)).ToListAsync(cancellationToken);
        return rows.GroupBy(r => r.UserId)
                   .ToDictionary(g => g.Key, g => (IReadOnlyList<StaffCountry>)g.ToList());
    }

    /// <inheritdoc />
    public Task<bool> HasAnyAsync(Guid userId, CancellationToken cancellationToken = default) =>
        Set.AnyAsync(s => s.UserId == userId, cancellationToken);
}
