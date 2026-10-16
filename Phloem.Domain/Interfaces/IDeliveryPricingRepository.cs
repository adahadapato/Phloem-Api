using Phloem.Domain.Entities;
using Phloem.Domain.Enums;

namespace Phloem.Domain.Interfaces;

/// <summary>
/// Loads and saves <see cref="DeliveryPricing"/> (one per country, with its window charges).
/// Implemented in Persistence with EF Core.
/// </summary>
public interface IDeliveryPricingRepository : IRepository<DeliveryPricing>
{
    /// <summary>The pricing for one country, with its window charges; null if not set up yet.</summary>
    /// <param name="country">The country.</param>
    /// <param name="cancellationToken">Cancels the database call.</param>
    Task<DeliveryPricing?> GetByCountryAsync(Country country, CancellationToken cancellationToken = default);

    /// <summary>Admin → Delivery pricing page: every country's pricing.</summary>
    /// <param name="cancellationToken">Cancels the database call.</param>
    Task<IReadOnlyList<DeliveryPricing>> GetAllAsync(CancellationToken cancellationToken = default);
}