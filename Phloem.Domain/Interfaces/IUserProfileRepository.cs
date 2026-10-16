using Phloem.Domain.Entities;
using Phloem.Domain.Shared.Results;

namespace Phloem.Domain.Interfaces;

public interface IUserProfileRepository : IRepository<UserProfile>
{
    // GetByIdAsync (inherited) includes the delivery addresses.

    /// <summary>
    /// Admin dashboard → Users tab: everyone who has signed up, newest first.
    /// Search matches the full name.
    /// </summary>
    Task<PagedResult<UserProfile>> GetPagedAsync(
        string? search, PageRequest page, CancellationToken cancellationToken = default);

    /// <summary>
    /// Admin lists: people's names for a whole list in ONE query (Id → full name).
    /// Ids not found are simply missing; callers show "Unknown user".
    /// </summary>
    Task<IReadOnlyDictionary<Guid, string>> GetFullNamesAsync(
        IReadOnlyCollection<Guid> userIds, CancellationToken cancellationToken = default);
}