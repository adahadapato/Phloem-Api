using Microsoft.EntityFrameworkCore;
using Phloem.Domain.Entities;
using Phloem.Domain.Enums;
using Phloem.Domain.Interfaces;
using Phloem.Domain.Shared.Results;

namespace Phloem.Persistence.Repositories;

/// <summary>EF Core implementation of <see cref="IShipmentRepository"/>.</summary>
/// <remarks>
/// <para><b>Aggregate:</b> a shipment is always loaded with its items and timeline. The <c>Sender</c> is never
/// Include-d (soft-deletable required parent); the shipment keeps SenderName / SenderPhone snapshots.</para>
/// <para>The timeline is sorted by the entity itself (<c>Shipment.Timeline</c>), so no ordering is needed here.</para>
/// </remarks>
internal sealed class ShipmentRepository(PhloemDbContext context)
    : Repository<Shipment>(context), IShipmentRepository
{
    /// <inheritdoc />
    protected override IQueryable<Shipment> WithChildren(IQueryable<Shipment> query) =>
        query.Include(s => s.Items)
             .Include(s => s.Timeline);

    // ───────────────────────────── 1. Single shipment ─────────────────────────────

    /// <inheritdoc />
    /// <remarks>Tracked: cancel, receive at hub and progress updates load the shipment through this.</remarks>
    public Task<Shipment?> GetByTrackingNumberAsync(string trackingNumber, CancellationToken cancellationToken = default)
    {
        var number = trackingNumber.Trim();
        return WithChildren(Set).FirstOrDefaultAsync(s => s.TrackingNumber == number, cancellationToken);
    }

    /// <inheritdoc />
    public Task<bool> TrackingNumberExistsAsync(string trackingNumber, CancellationToken cancellationToken = default)
    {
        var number = trackingNumber.Trim();
        return Set.IgnoreQueryFilters().AnyAsync(s => s.TrackingNumber == number, cancellationToken);
    }

    // ───────────────────────────── 2. Lists ─────────────────────────────

    /// <inheritdoc />
    public Task<PagedResult<Shipment>> GetForSenderAsync(Guid senderId, PageRequest page,
                                                         CancellationToken cancellationToken = default)
    {
        var query = WithChildren(Set.AsNoTracking())
            .Where(s => s.SenderId == senderId)
            .OrderByDescending(s => s.CreatedAt);

        return ToPagedAsync(query, page, cancellationToken);
    }

    /// <inheritdoc />
    public Task<PagedResult<Shipment>> GetPagedAsync(ShipmentStatus? status, Country? fromCountry, Country? toCountry,
                                                     PageRequest page, CancellationToken cancellationToken = default)
    {
        IQueryable<Shipment> query = WithChildren(Set.AsNoTracking());

        if (status is not null) query = query.Where(s => s.Status == status);
        if (fromCountry is not null) query = query.Where(s => s.FromCountry == fromCountry);
        if (toCountry is not null) query = query.Where(s => s.ToCountry == toCountry);

        return ToPagedAsync(query.OrderByDescending(s => s.CreatedAt), page, cancellationToken);
    }
}