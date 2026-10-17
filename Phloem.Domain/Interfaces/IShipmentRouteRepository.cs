using Phloem.Domain.Entities;
using Phloem.Domain.Enums;

namespace Phloem.Domain.Interfaces;

/// <summary>
/// Loads and saves <see cref="ShipmentRoute"/>s (with their rates). Implemented in Persistence.
/// </summary>
public interface IShipmentRouteRepository : IRepository<ShipmentRoute>
{
    /// <summary>The route between two countries (in that direction), with rates; null if none.</summary>
    /// <param name="fromCountry">Origin.</param>
    /// <param name="toCountry">Destination.</param>
    /// <param name="cancellationToken">Cancels the database call.</param>
    Task<ShipmentRoute?> GetByCountriesAsync(Country fromCountry, Country toCountry,
                                             CancellationToken cancellationToken = default);

    /// <summary>"Send a parcel" step 1: switched-on routes only, with rates.</summary>
    /// <param name="cancellationToken">Cancels the database call.</param>
    Task<IReadOnlyList<ShipmentRoute>> GetActiveAsync(CancellationToken cancellationToken = default);

    /// <summary>Admin → Shipping routes: every route, on or off, with rates.</summary>
    /// <param name="cancellationToken">Cancels the database call.</param>
    Task<IReadOnlyList<ShipmentRoute>> GetAllAsync(CancellationToken cancellationToken = default);
}