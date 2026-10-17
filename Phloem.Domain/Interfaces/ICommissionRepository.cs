using Phloem.Domain.Entities;
using Phloem.Domain.Enums;

namespace Phloem.Domain.Interfaces;

/// <summary>Loads and saves <see cref="Commission"/> rates (one per country). Implemented in Persistence.</summary>
public interface ICommissionRepository : IRepository<Commission>
{
    /// <summary>The rate for one country; null = none set (treated as 0%).</summary>
    /// <param name="country">The country.</param>
    /// <param name="cancellationToken">Cancels the database call.</param>
    Task<Commission?> GetByCountryAsync(Country country, CancellationToken cancellationToken = default);

    /// <summary>Every country's rate (admin page).</summary>
    /// <param name="cancellationToken">Cancels the database call.</param>
    Task<IReadOnlyList<Commission>> GetAllAsync(CancellationToken cancellationToken = default);
}