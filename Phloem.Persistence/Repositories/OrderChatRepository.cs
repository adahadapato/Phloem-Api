using Microsoft.EntityFrameworkCore;
using Phloem.Domain.Entities;
using Phloem.Domain.Enums;
using Phloem.Domain.Interfaces;
using Phloem.Domain.Shared.Results;

namespace Phloem.Persistence.Repositories;

/// <summary><see cref="IOrderChatRepository"/> with EF Core.</summary>
internal sealed class OrderChatRepository(PhloemDbContext context)
    : Repository<OrderChat>(context), IOrderChatRepository
{
    private DbSet<ChatMessage> Messages => Context.Set<ChatMessage>();

    /// <inheritdoc />
    public Task<OrderChat?> GetByOrderIdAsync(Guid orderId, CancellationToken cancellationToken = default) =>
        Set.FirstOrDefaultAsync(c => c.OrderId == orderId, cancellationToken);

    /// <inheritdoc />
    public void AddMessage(ChatMessage message) => Messages.Add(message);

    /// <inheritdoc />
    public Task<ChatMessage?> GetMessageAsync(Guid messageId, CancellationToken cancellationToken = default) =>
        Messages.FirstOrDefaultAsync(m => m.Id == messageId, cancellationToken);

    /// <inheritdoc />
    public async Task<OrderChat?> GetChatOfMessageAsync(Guid messageId, CancellationToken cancellationToken = default)
    {
        var chatId = await Messages.Where(m => m.Id == messageId).Select(m => (Guid?)m.ChatId)
                                   .FirstOrDefaultAsync(cancellationToken);
        return chatId is null ? null : await Set.AsNoTracking().FirstOrDefaultAsync(c => c.Id == chatId, cancellationToken);
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<ChatMessage>> GetMessagesAsync(Guid chatId, DateTime? beforeUtc, int take,
                                                                   CancellationToken cancellationToken = default)
    {
        var query = Messages.AsNoTracking().Where(m => m.ChatId == chatId);
        if (beforeUtc is { } before) query = query.Where(m => m.CreatedAt < before);
        return await query.OrderByDescending(m => m.CreatedAt).Take(take).ToListAsync(cancellationToken);
    }

    /// <inheritdoc />
    public Task<PagedResult<OrderChat>> GetForUserAsync(Guid userId, PageRequest page,
                                                        CancellationToken cancellationToken = default)
    {
        var query = Set.AsNoTracking()
                       .Where(c => c.CustomerId == userId || c.ShopOwnerId == userId)
                       .OrderByDescending(c => c.LastCustomerMessageAtUtc > c.LastSellerMessageAtUtc
                                                   || c.LastSellerMessageAtUtc == null
                                                   ? c.LastCustomerMessageAtUtc
                                                   : c.LastSellerMessageAtUtc);
        return ToPagedAsync(query, page, cancellationToken);
    }

    /// <inheritdoc />
    public async Task<IReadOnlyDictionary<Guid, ChatMessage>> GetLastMessagesAsync(IReadOnlyCollection<Guid> chatIds,
                                                                                   CancellationToken cancellationToken = default)
    {
        if (chatIds.Count == 0) return new Dictionary<Guid, ChatMessage>();
        var ids = chatIds.ToList();
        var last = await Messages.AsNoTracking()
            .Where(m => ids.Contains(m.ChatId))
            .GroupBy(m => m.ChatId)
            .Select(g => g.OrderByDescending(m => m.CreatedAt).First())
            .ToListAsync(cancellationToken);
        return last.ToDictionary(m => m.ChatId);
    }

    /// <inheritdoc />
    public Task<int> CountUnreadForUserAsync(Guid userId, CancellationToken cancellationToken = default) =>
        Set.AsNoTracking().CountAsync(c =>
            (c.CustomerId == userId && c.LastSellerMessageAtUtc != null
             && (c.CustomerReadUpToUtc == null || c.CustomerReadUpToUtc < c.LastSellerMessageAtUtc))
            || (c.ShopOwnerId == userId && c.LastCustomerMessageAtUtc != null
                && (c.SellerReadUpToUtc == null || c.SellerReadUpToUtc < c.LastCustomerMessageAtUtc)),
            cancellationToken);

    /// <inheritdoc />
    public Task<PagedResult<OrderChat>> GetAdminPagedAsync(bool reportedOnly, IReadOnlyCollection<Country>? countries,
                                                           PageRequest page, CancellationToken cancellationToken = default)
    {
        IQueryable<OrderChat> query = Set.AsNoTracking();
        if (countries is not null)
        {
            var covered = countries.ToList();
            query = query.Where(c => covered.Contains(c.Country));
        }
        if (reportedOnly) query = query.Where(c => c.IsReported);

        return ToPagedAsync(query.OrderByDescending(c => c.IsReported && c.ReportHandledAtUtc == null)
                                 .ThenByDescending(c => c.ReportedAtUtc)
                                 .ThenByDescending(c => c.CreatedAt), page, cancellationToken);
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<OrderChat>> GetMaybeNeedingEmailAsync(DateTime cutoffUtc, int take,
                                                                          CancellationToken cancellationToken = default) =>
        await Set.Where(c =>
                    (c.LastSellerMessageAtUtc != null && c.LastSellerMessageAtUtc <= cutoffUtc
                     && (c.CustomerReadUpToUtc == null || c.CustomerReadUpToUtc < c.LastSellerMessageAtUtc)
                     && (c.CustomerEmailedUpToUtc == null || c.CustomerEmailedUpToUtc < c.LastSellerMessageAtUtc))
                    || (c.LastCustomerMessageAtUtc != null && c.LastCustomerMessageAtUtc <= cutoffUtc
                        && (c.SellerReadUpToUtc == null || c.SellerReadUpToUtc < c.LastCustomerMessageAtUtc)
                        && (c.SellerEmailedUpToUtc == null || c.SellerEmailedUpToUtc < c.LastCustomerMessageAtUtc)))
                 .Take(take)
                 .ToListAsync(cancellationToken);
}
