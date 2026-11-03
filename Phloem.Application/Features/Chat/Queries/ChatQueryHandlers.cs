using MediatR;
using Phloem.Application.Common.Authorization;
using Phloem.Application.Features.Chat.Common;
using Phloem.Application.Features.Chat.Dtos;
using Phloem.Domain.Entities;
using Phloem.Domain.Enums;
using Phloem.Domain.Interfaces;
using Phloem.Domain.Rules;
using Phloem.Domain.Shared.Errors;
using Phloem.Domain.Shared.Results;

namespace Phloem.Application.Features.Chat.Queries;

/// <summary>Handles <see cref="GetOrderChatQuery"/>.</summary>
public sealed class GetOrderChatQueryHandler(
    ICurrentUser currentUser,
    ChatAccess access,
    IOrderChatRepository chats,
    ChatFormatting format,
    TimeProvider time)
    : IRequestHandler<GetOrderChatQuery, Result<OrderChatDto>>
{
    private const int PageSize = 50;

    /// <returns>The chat (empty when nobody has written yet); or <see cref="ChatErrors.NotFound"/>.</returns>
    public async Task<Result<OrderChatDto>> Handle(GetOrderChatQuery query, CancellationToken ct)
    {
        var userId = currentUser.UserId ?? throw new InvalidOperationException("This action requires a signed-in user.");
        var found = await access.ForParticipantAsync(query.OrderNumber, userId, ct);
        if (found.IsFailure) return found.Error;
        var (order, chat, role, ownerId) = found.Value;

        var (customerName, ownerName) = await format.FullNamesAsync(order.CustomerId, ownerId, ct);
        var otherName = role == ChatRole.Customer
            ? ChatFormatting.PublicName(ownerName, ChatRole.Seller, order.VendorName)
            : ChatFormatting.PublicName(customerName, ChatRole.Customer, order.VendorName);

        var messages = new List<ChatMessageDto>();
        var hasOlder = false;
        if (chat is not null)
        {
            var rows = await chats.GetMessagesAsync(chat.Id, null, PageSize + 1, ct);
            hasOlder = rows.Count > PageSize;
            foreach (var m in rows.Take(PageSize).Reverse())
            {
                if (m.HiddenBySender && m.SenderId == userId) continue;   // "Delete" hides it for the sender only
                messages.Add(await format.ForParticipantAsync(m, customerName, ownerName, order.VendorName, ct));
            }
        }

        var open = OrderChat.IsOpen(order, time.GetUtcNow().UtcDateTime);
        return new OrderChatDto(
            order.OrderNumber, order.VendorName, role, otherName, open,
            open ? null : $"This chat closed {OrderChat.OpenDaysAfterOrder} days after the order ended. " +
                          "If something is wrong, use Contact us.",
            chat?.ReportWaiting ?? false,
            chat?.ReadUpTo(role == ChatRole.Customer ? ChatRole.Seller : ChatRole.Customer),
            messages, hasOlder, ChatFormatting.Notice, OffPlatformHints.Warning);
    }
}

/// <summary>Handles <see cref="GetOlderChatMessagesQuery"/>.</summary>
public sealed class GetOlderChatMessagesQueryHandler(
    ICurrentUser currentUser,
    ChatAccess access,
    IOrderChatRepository chats,
    ChatFormatting format)
    : IRequestHandler<GetOlderChatMessagesQuery, Result<IReadOnlyList<ChatMessageDto>>>
{
    private const int PageSize = 50;

    /// <returns>Up to 50 earlier messages, oldest first (empty when there are no more).</returns>
    public async Task<Result<IReadOnlyList<ChatMessageDto>>> Handle(GetOlderChatMessagesQuery query, CancellationToken ct)
    {
        var userId = currentUser.UserId ?? throw new InvalidOperationException("This action requires a signed-in user.");
        var found = await access.ForParticipantAsync(query.OrderNumber, userId, ct);
        if (found.IsFailure) return found.Error;
        var (order, chat, _, ownerId) = found.Value;
        if (chat is null) return Array.Empty<ChatMessageDto>();

        var (customerName, ownerName) = await format.FullNamesAsync(order.CustomerId, ownerId, ct);
        var result = new List<ChatMessageDto>();
        foreach (var m in (await chats.GetMessagesAsync(chat.Id, query.Before, PageSize, ct)).Reverse())
        {
            if (m.HiddenBySender && m.SenderId == userId) continue;
            result.Add(await format.ForParticipantAsync(m, customerName, ownerName, order.VendorName, ct));
        }
        return result;
    }
}

/// <summary>Handles <see cref="GetMyChatsQuery"/>.</summary>
public sealed class GetMyChatsQueryHandler(ICurrentUser currentUser, IOrderChatRepository chats,
                                           IUserProfileRepository profiles)
    : IRequestHandler<GetMyChatsQuery, Result<PagedResult<MyChatDto>>>
{
    /// <summary>My conversations, with the other person's name and the last message.</summary>
    public async Task<Result<PagedResult<MyChatDto>>> Handle(GetMyChatsQuery query, CancellationToken ct)
    {
        var userId = currentUser.UserId ?? throw new InvalidOperationException("This action requires a signed-in user.");
        var page = new PageRequest { Page = query.Page, PageSize = query.PageSize };
        var paged = await chats.GetForUserAsync(userId, page, ct);

        var last = await chats.GetLastMessagesAsync(paged.Items.Select(c => c.Id).ToList(), ct);
        var people = paged.Items.SelectMany(c => new[] { c.CustomerId, c.ShopOwnerId }).Distinct().ToList();
        var names = await profiles.GetFullNamesAsync(people, ct);

        var items = paged.Items.Select(c =>
        {
            var role = c.RoleOf(userId) ?? ChatRole.Customer;
            var other = c.OtherParty(role);
            var otherName = ChatFormatting.PublicName(names.GetValueOrDefault(other, "Someone"),
                                                      role == ChatRole.Customer ? ChatRole.Seller : ChatRole.Customer,
                                                      c.ShopName);
            var lastMessage = last.GetValueOrDefault(c.Id);
            var mineHidden = lastMessage is { HiddenBySender: true } && lastMessage.SenderId == userId;
            return new MyChatDto(c.OrderNumber, c.ShopName, role, otherName,
                                 lastMessage is null || mineHidden ? null : ChatFormatting.Preview(lastMessage),
                                 lastMessage?.SenderId == userId, c.LastMessageAtUtc, c.HasUnread(role));
        }).ToList();

        return new PagedResult<MyChatDto>(items, paged.TotalCount, paged.Page, paged.PageSize);
    }
}

/// <summary>Handles <see cref="GetMyUnreadChatCountQuery"/>.</summary>
public sealed class GetMyUnreadChatCountQueryHandler(ICurrentUser currentUser, IOrderChatRepository chats)
    : IRequestHandler<GetMyUnreadChatCountQuery, Result<int>>
{
    /// <summary>Chats with unread messages.</summary>
    public async Task<Result<int>> Handle(GetMyUnreadChatCountQuery query, CancellationToken ct) =>
        currentUser.UserId is { } userId ? await chats.CountUnreadForUserAsync(userId, ct) : 0;
}

/// <summary>Handles <see cref="GetChatParticipantsQuery"/>.</summary>
public sealed class GetChatParticipantsQueryHandler(ChatAccess access)
    : IRequestHandler<GetChatParticipantsQuery, Result<ChatParticipants>>
{
    /// <returns>The two people; or <see cref="ChatErrors.NotFound"/> when the asker isn't one of them.</returns>
    public async Task<Result<ChatParticipants>> Handle(GetChatParticipantsQuery query, CancellationToken ct)
    {
        var found = await access.ForParticipantAsync(query.OrderNumber, query.UserId, ct);
        if (found.IsFailure) return found.Error;
        return new ChatParticipants(found.Value.Order.CustomerId, found.Value.ShopOwnerId, found.Value.Role);
    }
}

/// <summary>Handles <see cref="GetAdminChatsQuery"/>.</summary>
public sealed class GetAdminChatsQueryHandler(ICurrentUser currentUser, IOrderChatRepository chats,
                                              IUserProfileRepository profiles)
    : IRequestHandler<GetAdminChatsQuery, Result<PagedResult<AdminChatSummaryDto>>>
{
    /// <summary>One page of chats in the person's countries.</summary>
    public async Task<Result<PagedResult<AdminChatSummaryDto>>> Handle(GetAdminChatsQuery query, CancellationToken ct)
    {
        var page = new PageRequest { Page = query.Page, PageSize = query.PageSize };
        var paged = await chats.GetAdminPagedAsync(query.ReportedOnly ?? false, currentUser.CountryLimit(), page, ct);
        var people = paged.Items.SelectMany(c => new[] { c.CustomerId, c.ShopOwnerId })
                                .Concat(paged.Items.Where(c => c.ReportedBy is not null).Select(c => c.ReportedBy!.Value))
                                .Distinct().ToList();
        var names = await profiles.GetFullNamesAsync(people, ct);

        var items = paged.Items.Select(c => AdminChatMapping.Summary(c, names)).ToList();
        return new PagedResult<AdminChatSummaryDto>(items, paged.TotalCount, paged.Page, paged.PageSize);
    }
}

/// <summary>Handles <see cref="GetAdminChatQuery"/>.</summary>
public sealed class GetAdminChatQueryHandler(
    ICurrentUser currentUser,
    IOrderRepository orders,
    IOrderChatRepository chats,
    IUserProfileRepository profiles,
    ChatFormatting format,
    IStaffAuditRepository audit,
    IUnitOfWork unitOfWork)
    : IRequestHandler<GetAdminChatQuery, Result<AdminChatDto>>
{
    /// <returns>The whole chat; or <see cref="ChatErrors.NotFound"/> (no chat, or outside the person's countries).</returns>
    public async Task<Result<AdminChatDto>> Handle(GetAdminChatQuery query, CancellationToken ct)
    {
        var staffId = currentUser.UserId ?? throw new InvalidOperationException("This action requires a signed-in user.");
        var order = await orders.GetByOrderNumberAsync(query.OrderNumber.Trim(), ct);
        var chat = order is null ? null : await chats.GetByOrderIdAsync(order.Id, ct);
        if (chat is null || !currentUser.CoversCountry(chat.Country)) return ChatErrors.NotFound;

        // People's private conversation: every opening is recorded
        audit.Add(StaffAuditEntry.Create(staffId, StaffAction.ChatViewed, $"Chat {chat.OrderNumber}", chat.Country,
                                         chat.ReportWaiting ? "Reported chat" : null));
        await unitOfWork.SaveChangesAsync(ct);

        var people = new List<Guid> { chat.CustomerId, chat.ShopOwnerId };
        if (chat.ReportedBy is { } by) people.Add(by);
        var names = await profiles.GetFullNamesAsync(people, ct);
        var customerName = names.GetValueOrDefault(chat.CustomerId, "Customer");
        var ownerName = names.GetValueOrDefault(chat.ShopOwnerId, "Shop");

        var messages = new List<ChatMessageDto>();
        DateTime? before = null;
        while (true)   // every message, in pages of 200 (chats are short)
        {
            var rows = await chats.GetMessagesAsync(chat.Id, before, 200, ct);
            foreach (var m in rows) messages.Add(await format.ForStaffAsync(m, customerName, ownerName, ct));
            if (rows.Count < 200) break;
            before = rows[^1].CreatedAt;
        }
        messages.Reverse();

        return new AdminChatDto(AdminChatMapping.Summary(chat, names), ownerName, messages);
    }
}

/// <summary>Admin chat rows.</summary>
internal static class AdminChatMapping
{
    /// <summary>One row, with full names.</summary>
    public static AdminChatSummaryDto Summary(OrderChat c, IReadOnlyDictionary<Guid, string> names) => new(
        c.OrderNumber, c.ShopName, names.GetValueOrDefault(c.CustomerId, "Customer"), c.Country, c.IsReported,
        c.ReportWaiting, c.ReportReason,
        c.ReportedBy is { } by
            ? $"{names.GetValueOrDefault(by, "Someone")} ({(by == c.CustomerId ? "customer" : "shop")})"
            : null,
        c.ReportedAtUtc, c.LastMessageAtUtc);
}
