using Phloem.Domain.Entities;
using Phloem.Domain.Enums;
using Phloem.Domain.Shared.Results;

namespace Phloem.Domain.Interfaces;

public interface IVendorRepository : IRepository<Vendor>
{
    /// <summary>
    /// "Sell food" button / Seller dashboard / Shop settings: the signed-in user's own shop.
    /// Null means they haven't opened one yet → Seller setup page.
    /// </summary>
    Task<Vendor?> GetByOwnerIdAsync(Guid ownerId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Seller setup + Shop settings: "Another shop already uses that name".
    /// Case-insensitive. Pass the shop's own Id when renaming so it doesn't clash with itself.
    /// </summary>
    Task<bool> NameExistsAsync(
        string name, Guid? exceptVendorId = null, CancellationToken cancellationToken = default);

    /// <summary>Admin dashboard → "Waiting for approval" list, oldest first.</summary>
    Task<IReadOnlyList<Vendor>> GetPendingApprovalAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Vendors list page (buyers: <paramref name="visibleToBuyersOnly"/> = approved shops whose owner isn't
    /// suspended) and Admin "Active sellers" (any status). Optional filters: status, country, name search.
    /// <paramref name="coveredCountries"/> limits staff to their countries (null = every country).
    /// </summary>
    Task<PagedResult<Vendor>> GetPagedAsync(
        VendorStatus? status, Country? country, string? search, PageRequest page,
        CancellationToken cancellationToken = default, bool visibleToBuyersOnly = false,
        IReadOnlyCollection<Country>? coveredCountries = null);

    /// <summary>Home page → "Top sellers": best-rated approved shops, optionally in one country.</summary>
    Task<IReadOnlyList<Vendor>> GetTopRatedAsync(
        Country? country, int count, CancellationToken cancellationToken = default);

    /// <summary>Admin dashboard totals: how many shops in each status (closed shops excluded), only in
    /// <paramref name="coveredCountries"/> when given (null = every country).</summary>
    Task<IReadOnlyDictionary<VendorStatus, int>> CountByStatusAsync(
        CancellationToken cancellationToken = default, IReadOnlyCollection<Country>? coveredCountries = null);

    /// <summary>
    /// Admin lists: shop names for a whole list of owners in ONE query (owner Id → shop name).
    /// Owners without a shop are simply missing.
    /// </summary>
    Task<IReadOnlyDictionary<Guid, string>> GetNamesByOwnerIdsAsync(
        IReadOnlyCollection<Guid> ownerIds, CancellationToken cancellationToken = default);

    /// <summary>
    /// Admin changed a country's delivery fee range: every shop in that country (not closed),
    /// so each fee can be moved into the new range.
    /// </summary>
    /// <param name="country">The country.</param>
    /// <param name="cancellationToken">Cancels the database call.</param>
    Task<IReadOnlyList<Vendor>> GetByCountryAsync(Country country, CancellationToken cancellationToken = default);

    /// <summary>Every shop buyers can see (approved, owner not suspended), as SEO needs it (read-only).</summary>
    /// <param name="cancellationToken">Stops the query.</param>
    Task<IReadOnlyList<Models.SeoShop>> GetVisibleForSeoAsync(CancellationToken cancellationToken = default);
}
