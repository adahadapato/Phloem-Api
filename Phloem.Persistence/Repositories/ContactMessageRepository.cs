using Microsoft.EntityFrameworkCore;
using Phloem.Domain.Entities;
using Phloem.Domain.Enums;
using Phloem.Domain.Interfaces;
using Phloem.Domain.Shared.Results;

namespace Phloem.Persistence.Repositories;

/// <summary>EF Core implementation of <see cref="IContactMessageRepository"/>.</summary>
internal sealed class ContactMessageRepository(PhloemDbContext context)
    : Repository<ContactMessage>(context), IContactMessageRepository
{
    /// <inheritdoc />
    public Task<PagedResult<ContactMessage>> GetPageAsync(ContactMessageStatus? status, PageRequest page,
                                                          CancellationToken cancellationToken = default)
    {
        var query = Set.AsNoTracking();
        // "All" means the inbox: everything except spam (spam has its own tab)
        query = status is not null
            ? query.Where(m => m.Status == status)
            : query.Where(m => m.Status != ContactMessageStatus.Spam);
        return ToPagedAsync(query.OrderByDescending(m => m.CreatedAt), page, cancellationToken);
    }

    /// <inheritdoc />
    public async Task<IReadOnlyDictionary<ContactMessageStatus, int>> CountByStatusAsync(
        CancellationToken cancellationToken = default) =>
        await Set.GroupBy(m => m.Status)
                 .Select(g => new { g.Key, Count = g.Count() })
                 .ToDictionaryAsync(x => x.Key, x => x.Count, cancellationToken);
}
