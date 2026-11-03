using Phloem.Domain.Enums;

namespace Phloem.Application.Features.Chat.Dtos;

/// <summary>One chat message.</summary>
/// <param name="Id">The message.</param>
/// <param name="SenderId">Who sent it (the app compares it with the signed-in person to put it on the right).</param>
/// <param name="SenderRole">Customer or shop.</param>
/// <param name="SenderName">First name and role, e.g. "Ama (Mama's Kitchen)"; staff see full names.</param>
/// <param name="Body">The text (plain text: show it as text, never as HTML).</param>
/// <param name="PhotoUrl">A short-lived link to the photo; null when there's none.</param>
/// <param name="SentAt">When (UTC, server time).</param>
/// <param name="HiddenBySender">Staff only: the sender chose "Delete" (hidden for them).</param>
/// <param name="HasContactDetails">Staff only: the text looked like it shared contact or payment details.</param>
public sealed record ChatMessageDto(
    Guid Id,
    Guid SenderId,
    ChatRole SenderRole,
    string SenderName,
    string Body,
    string? PhotoUrl,
    DateTime SentAt,
    bool HiddenBySender,
    bool HasContactDetails);

/// <summary>An order's chat, as the customer or the shop sees it.</summary>
/// <param name="OrderNumber">The order.</param>
/// <param name="ShopName">The shop.</param>
/// <param name="MyRole">Which side the viewer is on.</param>
/// <param name="OtherPartyName">Who they're talking to, e.g. "Ama (Mama's Kitchen)" or "Grace (customer)".</param>
/// <param name="CanWrite">The chat is open.</param>
/// <param name="ClosedReason">Why it's read-only; null when open.</param>
/// <param name="IsReported">Sent to Phloem's team and waiting.</param>
/// <param name="OtherReadUpTo">The other side has seen messages up to here ("Seen" ticks).</param>
/// <param name="Messages">The newest messages, oldest first.</param>
/// <param name="HasOlder">There are earlier messages ("Show earlier").</param>
/// <param name="Notice">The line at the top ("Messages are saved…").</param>
/// <param name="ContactWarning">The warning to show before sending contact or payment details.</param>
public sealed record OrderChatDto(
    string OrderNumber,
    string ShopName,
    ChatRole MyRole,
    string OtherPartyName,
    bool CanWrite,
    string? ClosedReason,
    bool IsReported,
    DateTime? OtherReadUpTo,
    IReadOnlyList<ChatMessageDto> Messages,
    bool HasOlder,
    string Notice,
    string ContactWarning);

/// <summary>One conversation in "Messages".</summary>
/// <param name="OrderNumber">The order.</param>
/// <param name="ShopName">The shop.</param>
/// <param name="MyRole">Which side the viewer is on.</param>
/// <param name="OtherPartyName">Who it's with.</param>
/// <param name="LastMessage">The last message's text ("📷 Photo" for a photo).</param>
/// <param name="LastMessageIsMine">The viewer wrote the last message.</param>
/// <param name="LastMessageAt">When.</param>
/// <param name="Unread">Has messages the viewer hasn't seen.</param>
public sealed record MyChatDto(
    string OrderNumber,
    string ShopName,
    ChatRole MyRole,
    string OtherPartyName,
    string? LastMessage,
    bool LastMessageIsMine,
    DateTime? LastMessageAt,
    bool Unread);

/// <summary>Admin → Order chats: one row.</summary>
/// <param name="OrderNumber">The order.</param>
/// <param name="ShopName">The shop.</param>
/// <param name="CustomerName">The customer's full name.</param>
/// <param name="Country">Where the order goes.</param>
/// <param name="IsReported">Was reported.</param>
/// <param name="ReportWaiting">Reported and not dealt with yet.</param>
/// <param name="ReportReason">Why.</param>
/// <param name="ReportedBy">Who (full name and side).</param>
/// <param name="ReportedAt">When.</param>
/// <param name="LastMessageAt">The last message.</param>
public sealed record AdminChatSummaryDto(
    string OrderNumber,
    string ShopName,
    string CustomerName,
    Country Country,
    bool IsReported,
    bool ReportWaiting,
    string? ReportReason,
    string? ReportedBy,
    DateTime? ReportedAt,
    DateTime? LastMessageAt);

/// <summary>Admin → one order chat: every message, including ones the sender deleted (opening it is audited).</summary>
/// <param name="Chat">The summary.</param>
/// <param name="ShopOwnerName">Who writes for the shop (full name).</param>
/// <param name="Messages">Every message, oldest first.</param>
public sealed record AdminChatDto(
    AdminChatSummaryDto Chat,
    string ShopOwnerName,
    IReadOnlyList<ChatMessageDto> Messages);

/// <summary>Sent live to the other people in the chat when it changes.</summary>
/// <param name="OrderNumber">The order.</param>
/// <param name="Message">The new message (for "chatMessage").</param>
public sealed record ChatMessageEvent(string OrderNumber, ChatMessageDto Message);

/// <summary>Sent live when one side has seen the messages ("Seen" ticks).</summary>
/// <param name="OrderNumber">The order.</param>
/// <param name="Role">Who has seen them.</param>
/// <param name="ReadUpTo">Up to when.</param>
public sealed record ChatReadEvent(string OrderNumber, ChatRole Role, DateTime ReadUpTo);
