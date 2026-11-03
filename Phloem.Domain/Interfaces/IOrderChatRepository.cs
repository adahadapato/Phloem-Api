using Phloem.Domain.Entities;
using Phloem.Domain.Enums;
using Phloem.Domain.Shared.Results;

namespace Phloem.Domain.Interfaces;

/// <summary>Order chats and their messages.</summary>
public interface IOrderChatRepository : IRepository<OrderChat>
{
    /// <summary>The chat of an order (tracked), or null when nobody has written yet.</summary>
    Task<OrderChat?> GetByOrderIdAsync(Guid orderId, CancellationToken cancellationToken = default);

    /// <summary>Adds a message (saved with the caller's save).</summary>
    void AddMessage(ChatMessage message);

    /// <summary>One message (tracked), or null.</summary>
    Task<ChatMessage?> GetMessageAsync(Guid messageId, CancellationToken cancellationToken = default);

    /// <summary>The chat a message belongs to (read-only), or null.</summary>
    Task<OrderChat?> GetChatOfMessageAsync(Guid messageId, CancellationToken cancellationToken = default);

    /// <summary>Messages, newest first (read-only).</summary>
    /// <param name="chatId">The chat.</param>
    /// <param name="beforeUtc">Only older than this (for "show earlier"); null = the newest.</param>
    /// <param name="take">How many.</param>
    /// <param name="cancellationToken">Stops the query.</param>
    Task<IReadOnlyList<ChatMessage>> GetMessagesAsync(Guid chatId, DateTime? beforeUtc, int take,
                                                      CancellationToken cancellationToken = default);

    /// <summary>The chats a person is in (customer or shop owner), latest message first (read-only).</summary>
    Task<PagedResult<OrderChat>> GetForUserAsync(Guid userId, PageRequest page, CancellationToken cancellationToken = default);

    /// <summary>The newest message of each chat (read-only).</summary>
    Task<IReadOnlyDictionary<Guid, ChatMessage>> GetLastMessagesAsync(IReadOnlyCollection<Guid> chatIds,
                                                                      CancellationToken cancellationToken = default);

    /// <summary>How many chats have messages this person hasn't seen.</summary>
    Task<int> CountUnreadForUserAsync(Guid userId, CancellationToken cancellationToken = default);

    /// <summary>Admin → Order chats: reported ones (waiting first) or all, in the person's countries (read-only).</summary>
    /// <param name="reportedOnly">Only reported chats.</param>
    /// <param name="countries">Only these countries; null = all.</param>
    /// <param name="page">Which page.</param>
    /// <param name="cancellationToken">Stops the query.</param>
    Task<PagedResult<OrderChat>> GetAdminPagedAsync(bool reportedOnly, IReadOnlyCollection<Country>? countries,
                                                    PageRequest page, CancellationToken cancellationToken = default);

    /// <summary>Chats with unread messages older than <paramref name="cutoffUtc"/> that may need an email (tracked).</summary>
    Task<IReadOnlyList<OrderChat>> GetMaybeNeedingEmailAsync(DateTime cutoffUtc, int take,
                                                             CancellationToken cancellationToken = default);
}
