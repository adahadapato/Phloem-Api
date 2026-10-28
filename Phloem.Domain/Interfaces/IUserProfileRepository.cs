using Phloem.Domain.Entities;
using Phloem.Domain.Shared.Results;

namespace Phloem.Domain.Interfaces;

public interface IUserProfileRepository : IRepository<UserProfile>
{
    // GetByIdAsync (inherited) includes the delivery addresses.

    /// <summary>
    /// Admin dashboard → Users tab: everyone who has signed up, newest first.
    /// Search matches the full name; <paramref name="suspendedOnly"/> shows only suspended people.
    /// </summary>
    Task<PagedResult<UserProfile>> GetPagedAsync(
        string? search, PageRequest page, CancellationToken cancellationToken = default, bool suspendedOnly = false);

    /// <summary>Hourly expiry task: people whose suspension end date has passed (tracked, for <c>Reinstate</c>).</summary>
    /// <param name="utcNow">The current time.</param>
    /// <param name="cancellationToken">Cancels the database call.</param>
    Task<IReadOnlyList<UserProfile>> GetEndedSuspensionsAsync(
        DateTime utcNow, CancellationToken cancellationToken = default);

    /// <summary>
    /// Admin lists: people's names for a whole list in ONE query (Id → full name).
    /// Ids not found are simply missing; callers show "Unknown user".
    /// </summary>
    Task<IReadOnlyDictionary<Guid, string>> GetFullNamesAsync(
        IReadOnlyCollection<Guid> userIds, CancellationToken cancellationToken = default);

    /// <summary>
    /// "Choose a courier": the profiles (read-only, no addresses) of these people who live in one country, by name.
    /// Deleted accounts are left out; the caller leaves out suspended people.
    /// </summary>
    /// <param name="userIds">The candidates (e.g. everyone with the Courier role).</param>
    /// <param name="country">Only people in this country.</param>
    /// <param name="cancellationToken">Cancels the database call.</param>
    Task<IReadOnlyList<UserProfile>> GetInCountryAsync(
        IReadOnlyCollection<Guid> userIds, Enums.Country country, CancellationToken cancellationToken = default);
}