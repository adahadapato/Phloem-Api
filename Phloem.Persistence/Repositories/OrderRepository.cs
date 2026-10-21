using Microsoft.EntityFrameworkCore;
using Phloem.Domain.Entities;
using Phloem.Domain.Enums;
using Phloem.Domain.Interfaces;
using Phloem.Domain.Shared.Results;

namespace Phloem.Persistence.Repositories;

/// <summary>EF Core implementation of <see cref="IOrderRepository"/>.</summary>
/// <remarks>
/// <para><b>Aggregate:</b> an order is always loaded with its lines. The <c>Customer</c> is never Include-d: it's a
/// soft-deletable required parent (see EntityConfiguration's remarks); the order keeps its own snapshots instead.</para>
/// <para><b>Sellers never see unpaid orders:</b> every vendor query drops <see cref="OrderStatus.AwaitingPayment"/>.</para>
/// </remarks>
internal sealed class OrderRepository(PhloemDbContext context)
    : Repository<Order>(context), IOrderRepository
{
    /// <inheritdoc />
    protected override IQueryable<Order> WithChildren(IQueryable<Order> query) =>
        query.Include(o => o.Lines);

    // ───────────────────────────── 1. Single order ─────────────────────────────

    /// <inheritdoc />
    /// <remarks>Tracked: accept, send out, deliver, cancel and pay all load the order through this.</remarks>
    public Task<Order?> GetByOrderNumberAsync(string orderNumber, CancellationToken cancellationToken = default)
    {
        var number = orderNumber.Trim();
        return WithChildren(Set).FirstOrDefaultAsync(o => o.OrderNumber == number, cancellationToken);
    }

    /// <inheritdoc />
    public Task<bool> OrderNumberExistsAsync(string orderNumber, CancellationToken cancellationToken = default)
    {
        var number = orderNumber.Trim();
        return Set.IgnoreQueryFilters().AnyAsync(o => o.OrderNumber == number, cancellationToken);
    }

    // ───────────────────────────── 2. Lists ─────────────────────────────

    /// <inheritdoc />
    /// <remarks>Read-only. Unpaid orders included: the customer must see them to pay.</remarks>
    public Task<PagedResult<Order>> GetForCustomerAsync(Guid customerId, PageRequest page,
                                                        CancellationToken cancellationToken = default)
    {
        var query = WithChildren(Set.AsNoTracking())
            .Where(o => o.CustomerId == customerId)
            .OrderByDescending(o => o.PlacedAtUtc);

        return ToPagedAsync(query, page, cancellationToken);
    }

    /// <inheritdoc />
    /// <remarks>Read-only. Asking for <see cref="OrderStatus.AwaitingPayment"/> returns nothing, by design.</remarks>
    public Task<PagedResult<Order>> GetForVendorAsync(Guid vendorId, OrderStatus? status, PageRequest page,
                                                      CancellationToken cancellationToken = default)
    {
        var query = WithChildren(ForVendor(vendorId, status).AsNoTracking())
            .OrderByDescending(o => o.PlacedAtUtc);

        return ToPagedAsync(query, page, cancellationToken);
    }

    /// <inheritdoc />
    public Task<int> CountForVendorAsync(Guid vendorId, OrderStatus? status,
                                         CancellationToken cancellationToken = default) =>
        ForVendor(vendorId, status).CountAsync(cancellationToken);

    /// <inheritdoc />
    /// <remarks>Tracked: the job marks price changes as notified and saves.</remarks>
    public async Task<IReadOnlyList<Order>> GetAwaitingPaymentAsync(int skip, int take,
                                                                    CancellationToken cancellationToken = default) =>
        await WithChildren(Set)
                .Where(o => o.Status == OrderStatus.AwaitingPayment)
                .OrderBy(o => o.PlacedAtUtc)
                .ThenBy(o => o.Id)          // stable batches
                .Skip(skip)
                .Take(take)
                .ToListAsync(cancellationToken);

    /// <inheritdoc />
    public async Task<IReadOnlyList<Order>> GetUnfinishedForVendorAsync(Guid vendorId,
                                                                        CancellationToken cancellationToken = default) =>
        await WithChildren(Set)
                .Where(o => o.VendorId == vendorId
                         && (o.Status == OrderStatus.AwaitingPayment
                          || o.Status == OrderStatus.Paid
                          || o.Status == OrderStatus.Preparing))
                .ToListAsync(cancellationToken);

    // ───────────────────────────── 3. Dashboard ─────────────────────────────

    /// <inheritdoc />
    /// <remarks>Grouped in SQL by the UTC date; only the day/count pairs come back.</remarks>
    public async Task<IReadOnlyDictionary<DateOnly, int>> GetDailyPaidCountsForVendorAsync(
        Guid vendorId, DateOnly fromDate, DateOnly toDate, CancellationToken cancellationToken = default)
    {
        var from = fromDate.ToDateTime(TimeOnly.MinValue, DateTimeKind.Utc);
        var toExclusive = toDate.AddDays(1).ToDateTime(TimeOnly.MinValue, DateTimeKind.Utc);

        var rows = await Set.AsNoTracking()
            .Where(o => o.VendorId == vendorId
                     && o.Status != OrderStatus.Cancelled
                     && o.PaidAtUtc != null
                     && o.PaidAtUtc >= from
                     && o.PaidAtUtc < toExclusive)
            .GroupBy(o => o.PaidAtUtc!.Value.Date)
            .Select(g => new { Day = g.Key, Count = g.Count() })
            .ToListAsync(cancellationToken);

        return rows.ToDictionary(r => DateOnly.FromDateTime(r.Day), r => r.Count);
    }

    // ───────────────────────────── Helpers ─────────────────────────────

    /// <summary>A shop's orders as the seller may see them: never unpaid, optionally one status.</summary>
    private IQueryable<Order> ForVendor(Guid vendorId, OrderStatus? status)
    {
        var query = Set.Where(o => o.VendorId == vendorId && o.Status != OrderStatus.AwaitingPayment);
        return status is null ? query : query.Where(o => o.Status == status);
    }
}