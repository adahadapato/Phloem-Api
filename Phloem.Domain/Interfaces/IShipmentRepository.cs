using Phloem.Domain.Entities;
using Phloem.Domain.Enums;
using Phloem.Domain.Shared.Results;

namespace Phloem.Domain.Interfaces;

public interface IShipmentRepository : IRepository<Shipment>
{
    /// <summary>"Track my parcel": look up by the tracking number given to the sender.</summary>
    Task<Shipment?> GetByTrackingNumberAsync(string trackingNumber, CancellationToken cancellationToken = default);

    /// <summary>Tracking numbers must be unique: checked when generating a new one.</summary>
    Task<bool> TrackingNumberExistsAsync(string trackingNumber, CancellationToken cancellationToken = default);

    /// <summary>Send food to the UK → "My shipments": the sender's parcels, newest first.</summary>
    Task<PagedResult<Shipment>> GetForSenderAsync(
        Guid senderId, PageRequest page, CancellationToken cancellationToken = default);

    /// <summary>Admin / ShipmentManager → Shipments tab, optionally one status, newest first.</summary>
    Task<PagedResult<Shipment>> GetPagedAsync(
        ShipmentStatus? status, PageRequest page, CancellationToken cancellationToken = default);
}