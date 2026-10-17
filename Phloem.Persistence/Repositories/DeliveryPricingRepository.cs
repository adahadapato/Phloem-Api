using Microsoft.EntityFrameworkCore;
using Phloem.Domain.Entities;
using Phloem.Domain.Enums;
using Phloem.Domain.Interfaces;

namespace Phloem.Persistence.Repositories;

/// <summary>EF Core implementation of <see cref="IDeliveryPricingRepository"/>.</summary>
/// <remarks>A pricing is always loaded with its window charges (the aggregate).</remarks>
internal sealed class DeliveryPricingRepository(PhloemDbContext context)
    : Repository<DeliveryPricing>(context), IDeliveryPricingRepository
{
    /// <inheritdoc />
    protected override IQueryable<DeliveryPricing> WithChildren(IQueryable<DeliveryPricing> query) =>
        query.Include(p => p.WindowCharges);

    /// <inheritdoc />
    /// <remarks>Tracked: admins change it through this.</remarks>
    public Task<DeliveryPricing?> GetByCountryAsync(Country country, CancellationToken cancellationToken = default) =>
        WithChildren(Set).FirstOrDefaultAsync(p => p.Country == country, cancellationToken);

    /// <inheritdoc />
    public async Task<IReadOnlyList<DeliveryPricing>> GetAllAsync(CancellationToken cancellationToken = default) =>
        await WithChildren(Set.AsNoTracking()).OrderBy(p => p.Country).ToListAsync(cancellationToken);
}