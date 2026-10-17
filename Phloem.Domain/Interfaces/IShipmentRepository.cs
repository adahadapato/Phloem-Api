using Phloem.Domain.Entities;
using Phloem.Domain.Enums;
using Phloem.Domain.Shared.Results;

namespace Phloem.Domain.Interfaces;

/// <summary>
/// Loads and saves <see cref="Shipment"/> aggregates (a shipment with its items and timeline).
/// Implemented in Persistence.
/// </summary>
/// <remarks>Every method that returns a shipment includes its Items and Timeline.</remarks>
public interface IShipmentRepository : IRepository<Shipment>
{
    /// <summary>Tracking (sender's full view, or the public limited view), by tracking number.</summary>
    /// <param name="trackingNumber">The tracking number.</param>
    /// <param name="cancellationToken">Cancels the database call.</param>
    /// <returns>The shipment, or null if there is none.</returns>
    Task<Shipment?> GetByTrackingNumberAsync(string trackingNumber, CancellationToken cancellationToken = default);

    /// <summary>Checks a newly generated tracking number isn't already used.</summary>
    /// <param name="trackingNumber">The candidate number.</param>
    /// <param name="cancellationToken">Cancels the database call.</param>
    Task<bool> TrackingNumberExistsAsync(string trackingNumber, CancellationToken cancellationToken = default);

    /// <summary>"My shipments": the signed-in sender's parcels, newest first.</summary>
    /// <param name="senderId">The sender.</param>
    /// <param name="page">Which page and how many per page.</param>
    /// <param name="cancellationToken">Cancels the database call.</param>
    Task<PagedResult<Shipment>> GetForSenderAsync(
        Guid senderId, PageRequest page, CancellationToken cancellationToken = default);

    /// <summary>
    /// Admin / ShipmentManager → Shipments: optionally one status and/or one route direction, newest first.
    /// </summary>
    /// <param name="status">Only this status; null = all.</param>
    /// <param name="fromCountry">Only parcels from this country; null = all.</param>
    /// <param name="toCountry">Only parcels to this country; null = all.</param>
    /// <param name="page">Which page and how many per page.</param>
    /// <param name="cancellationToken">Cancels the database call.</param>
    Task<PagedResult<Shipment>> GetPagedAsync(
        ShipmentStatus? status, Country? fromCountry, Country? toCountry, PageRequest page,
        CancellationToken cancellationToken = default);
}