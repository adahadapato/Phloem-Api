using Phloem.Domain.Entities;

namespace Phloem.Domain.Interfaces;

/// <summary>The countries each staff member covers.</summary>
public interface IStaffCountryRepository : IRepository<StaffCountry>
{
    /// <summary>A staff member's rows (read-only); empty when they cover no country yet.</summary>
    /// <param name="userId">The staff member.</param>
    /// <param name="cancellationToken">Stops the query.</param>
    /// <returns>Their rows; one row with no country means "all countries".</returns>
    Task<IReadOnlyList<StaffCountry>> GetForUserAsync(Guid userId, CancellationToken cancellationToken = default);

    /// <summary>A staff member's rows, tracked so they can be removed.</summary>
    /// <param name="userId">The staff member.</param>
    /// <param name="cancellationToken">Stops the query.</param>
    /// <returns>Their rows, tracked.</returns>
    Task<IReadOnlyList<StaffCountry>> GetForUserToChangeAsync(Guid userId,
                                                              CancellationToken cancellationToken = default);

    /// <summary>True when the staff member has at least one row.</summary>
    /// <param name="userId">The staff member.</param>
    /// <param name="cancellationToken">Stops the query.</param>
    /// <returns>True when they cover at least one country.</returns>
    Task<bool> HasAnyAsync(Guid userId, CancellationToken cancellationToken = default);
}
