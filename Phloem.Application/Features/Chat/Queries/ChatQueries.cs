using MediatR;
using Phloem.Application.Features.Chat.Dtos;
using Phloem.Domain.Enums;
using Phloem.Domain.Shared.Results;

namespace Phloem.Application.Features.Chat.Queries;

/// <summary>The chat of one of my orders (customer or shop), newest messages. Never cached.</summary>
/// <param name="OrderNumber">The order.</param>
public sealed record GetOrderChatQuery(string OrderNumber) : IRequest<Result<OrderChatDto>>;

/// <summary>Earlier messages ("Show earlier").</summary>
/// <param name="OrderNumber">The order.</param>
/// <param name="Before">Messages sent before this (UTC).</param>
public sealed record GetOlderChatMessagesQuery(string OrderNumber, DateTime Before)
    : IRequest<Result<IReadOnlyList<ChatMessageDto>>>;

/// <summary>"Messages": my conversations, latest first.</summary>
/// <param name="Page">1-based page.</param>
/// <param name="PageSize">Chats per page.</param>
public sealed record GetMyChatsQuery(int Page = 1, int PageSize = 20) : IRequest<Result<PagedResult<MyChatDto>>>;

/// <summary>How many of my chats have unread messages (the number in the header).</summary>
public sealed record GetMyUnreadChatCountQuery : IRequest<Result<int>>;

/// <summary>For the live connection: the two people in an order's chat, if <paramref name="UserId"/> is one of them.</summary>
/// <param name="OrderNumber">The order.</param>
/// <param name="UserId">Who asks (from the connection, not the request).</param>
public sealed record GetChatParticipantsQuery(string OrderNumber, Guid UserId)
    : IRequest<Result<ChatParticipants>>;

/// <summary>The two people in a chat, and which side the asker is on.</summary>
public sealed record ChatParticipants(Guid CustomerId, Guid ShopOwnerId, ChatRole AskerRole);

/// <summary>Admin → Order chats: reported ones first (or only reported), in the person's countries.</summary>
/// <param name="ReportedOnly">Only reported chats.</param>
/// <param name="Page">1-based page.</param>
/// <param name="PageSize">Chats per page.</param>
public sealed record GetAdminChatsQuery(bool? ReportedOnly, int Page = 1, int PageSize = 20)
    : IRequest<Result<PagedResult<AdminChatSummaryDto>>>;

/// <summary>Admin → one order chat, every message. Opening it is written to the audit log.</summary>
/// <param name="OrderNumber">The order.</param>
public sealed record GetAdminChatQuery(string OrderNumber) : IRequest<Result<AdminChatDto>>;
