using Microsoft.EntityFrameworkCore;
using Phloem.Domain.Entities;
using Phloem.Domain.Interfaces;
using Phloem.Domain.Shared.Results;

namespace Phloem.Persistence.Repositories;

/// <summary>EF Core implementation of <see cref="IUserProfileRepository"/>.</summary>
/// <remarks>A profile is always loaded with its delivery addresses (the aggregate).</remarks>
internal sealed class UserProfileRepository(PhloemDbContext context)
    : Repository<UserProfile>(context), IUserProfileRepository
{
    /// <inheritdoc />
    protected override IQueryable<UserProfile> WithChildren(IQueryable<UserProfile> query) =>
        query.Include(p => p.Addresses);

    /// <inheritdoc />
    /// <remarks>Read-only list, newest first. Search matches first name, last name or "first last".</remarks>
    public Task<PagedResult<UserProfile>> GetPagedAsync(string? search, PageRequest page,
                                                         CancellationToken cancellationToken = default)
    {
        IQueryable<UserProfile> query = Set.AsNoTracking();

        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = search.Trim();
            query = query.Where(p => p.FirstName.Contains(term)
                                  || p.LastName.Contains(term)
                                  || (p.FirstName + " " + p.LastName).Contains(term));
        }

        return ToPagedAsync(query.OrderByDescending(p => p.CreatedAt), page, cancellationToken);
    }

    /// <inheritdoc />
    /// <remarks>
    /// Includes deleted accounts (shown as "Deleted user", their wiped name), so old orders and suggestions still
    /// read sensibly. One query; the name is joined in SQL.
    /// </remarks>
    public async Task<IReadOnlyDictionary<Guid, string>> GetFullNamesAsync(IReadOnlyCollection<Guid> userIds,
                                                                           CancellationToken cancellationToken = default)
    {
        if (userIds.Count == 0) return new Dictionary<Guid, string>();

        return await Set.IgnoreQueryFilters()
                        .AsNoTracking()
                        .Where(p => userIds.Contains(p.Id))
                        .Select(p => new { p.Id, Name = p.FirstName + " " + p.LastName })
                        .ToDictionaryAsync(p => p.Id, p => p.Name, cancellationToken);
    }
}