using Microsoft.EntityFrameworkCore;
using Phloem.Domain.Entities;
using Phloem.Domain.Enums;
using Phloem.Domain.Interfaces;

namespace Phloem.Persistence.Repositories;

/// <summary>EF Core implementation of <see cref="IShipmentRouteRepository"/>.</summary>
/// <remarks>A route is always loaded with its rates (the aggregate).</remarks>
internal sealed class ShipmentRouteRepository(PhloemDbContext context)
    : Repository<ShipmentRoute>(context), IShipmentRouteRepository
{
    /// <inheritdoc />
    protected override IQueryable<ShipmentRoute> WithChildren(IQueryable<ShipmentRoute> query) =>
        query.Include(r => r.Rates);

    /// <inheritdoc />
    /// <remarks>Tracked: every admin route command (rates, drop-off, on/off) loads the route through this.</remarks>
    public Task<ShipmentRoute?> GetByCountriesAsync(Country fromCountry, Country toCountry,
                                                    CancellationToken cancellationToken = default) =>
        WithChildren(Set).FirstOrDefaultAsync(r => r.FromCountry == fromCountry && r.ToCountry == toCountry,
                                              cancellationToken);

    /// <inheritdoc />
    public async Task<IReadOnlyList<ShipmentRoute>> GetActiveAsync(CancellationToken cancellationToken = default) =>
        await WithChildren(Set.AsNoTracking())
                .Where(r => r.IsActive)
                .OrderBy(r => r.FromCountry).ThenBy(r => r.ToCountry)
                .ToListAsync(cancellationToken);

    /// <inheritdoc />
    public async Task<IReadOnlyList<ShipmentRoute>> GetAllAsync(CancellationToken cancellationToken = default) =>
        await WithChildren(Set.AsNoTracking())
                .OrderBy(r => r.FromCountry).ThenBy(r => r.ToCountry)
                .ToListAsync(cancellationToken);
}