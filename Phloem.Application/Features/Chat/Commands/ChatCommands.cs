using MediatR;
using Phloem.Application.Features.Chat.Dtos;
using Phloem.Domain.Shared.Results;

namespace Phloem.Application.Features.Chat.Commands;

/// <summary>Writes in an order's chat: text, a photo, or both. Starts the chat with the first message.</summary>
/// <param name="OrderNumber">The order.</param>
/// <param name="Body">The text (may be empty with a photo).</param>
/// <param name="Photo">The photo upload, or <see cref="Stream.Null"/>.</param>
/// <param name="PhotoSizeBytes">Its size (0 = no photo).</param>
public sealed record SendChatMessageCommand(string OrderNumber, string? Body, Stream Photo, long PhotoSizeBytes)
    : IRequest<Result<ChatMessageDto>>;

/// <summary>I've seen the chat ("Seen" for the other side; clears my unread).</summary>
/// <param name="OrderNumber">The order.</param>
public sealed record MarkChatReadCommand(string OrderNumber) : IRequest<Result>;

/// <summary>"Delete" one of my messages: hidden for me only (the other side and staff still see it).</summary>
/// <param name="MessageId">The message.</param>
public sealed record HideChatMessageCommand(Guid MessageId) : IRequest<Result>;

/// <summary>"Report a problem": sends the chat to Phloem's team, with a reason.</summary>
/// <param name="OrderNumber">The order.</param>
/// <param name="Reason">What's wrong.</param>
public sealed record ReportChatCommand(string OrderNumber, string Reason) : IRequest<Result>;

/// <summary>Admin: the report has been dealt with.</summary>
/// <param name="OrderNumber">The order.</param>
public sealed record CloseChatReportCommand(string OrderNumber) : IRequest<Result>;

/// <summary>Background (every minute): emails people about messages they haven't seen for 5 minutes.</summary>
/// <returns>How many emails were sent.</returns>
public sealed record SendChatEmailsCommand : IRequest<Result<int>>;
