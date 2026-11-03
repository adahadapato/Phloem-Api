using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using Phloem.Application.Features.Chat.Queries;
using Phloem.Domain.Enums;
using Phloem.Infrastructure.Settings.Identity;

namespace Phloem.Presentation.Realtime;

/// <summary>
/// The live connection for order chats (<c>/hubs/chat</c>). Pages connect once and receive:
/// <list type="bullet">
///   <item><description><c>chatMessage</c> (<see cref="Application.Features.Chat.Dtos.ChatMessageEvent"/>): a new message;</description></item>
///   <item><description><c>chatRead</c> (<see cref="Application.Features.Chat.Dtos.ChatReadEvent"/>): the other side has seen up to a time;</description></item>
///   <item><description><c>chatTyping</c> (<see cref="ChatTypingEvent"/>): the other side is typing.</description></item>
/// </list>
/// Sending, reading and reporting go through the normal API (so they're checked, saved and logged the same way);
/// the only thing a page sends here is "I'm typing".
/// </summary>
/// <remarks>Messages go to people (every tab and phone they're signed in on), not to connections. The browser falls
/// back to long polling automatically where WebSockets are blocked.</remarks>
[Authorize]
public sealed class ChatHub(ISender sender) : Hub
{
    /// <summary>The event names pages listen for.</summary>
    public static class Events
    {
        /// <summary>A new message.</summary>
        public const string Message = "chatMessage";

        /// <summary>The other side has seen the chat.</summary>
        public const string Read = "chatRead";

        /// <summary>The other side is typing.</summary>
        public const string Typing = "chatTyping";
    }

    /// <summary>"I'm typing" in this order's chat (pages send it at most every few seconds).</summary>
    /// <param name="orderNumber">The order.</param>
    public async Task Typing(string orderNumber)
    {
        if (string.IsNullOrWhiteSpace(orderNumber) || orderNumber.Length > 30) return;
        if (!Guid.TryParse(Context.User?.FindFirst(JwtClaimNames.UserId)?.Value, out var userId)) return;

        var found = await sender.Send(new GetChatParticipantsQuery(orderNumber, userId), Context.ConnectionAborted);
        if (found.IsFailure) return;   // not theirs: say nothing

        var other = found.Value.AskerRole == ChatRole.Customer ? found.Value.ShopOwnerId : found.Value.CustomerId;
        await Clients.User(other.ToString())
                     .SendAsync(Events.Typing, new ChatTypingEvent(orderNumber.Trim(), found.Value.AskerRole),
                                Context.ConnectionAborted);
    }
}

/// <summary>The other side is typing.</summary>
/// <param name="OrderNumber">The order.</param>
/// <param name="Role">Who is typing.</param>
public sealed record ChatTypingEvent(string OrderNumber, ChatRole Role);

/// <summary>SignalR's "who is this connection": the user id in the token's <c>sub</c> claim.</summary>
public sealed class ChatUserIdProvider : IUserIdProvider
{
    /// <inheritdoc />
    public string? GetUserId(HubConnectionContext connection) =>
        connection.User?.FindFirst(JwtClaimNames.UserId)?.Value;
}

/// <summary>Sends chat events to people's open pages (<see cref="Application.Features.Chat.Common.IChatRealtime"/>).</summary>
public sealed class ChatRealtime(IHubContext<ChatHub> hub) : Application.Features.Chat.Common.IChatRealtime
{
    /// <inheritdoc />
    public Task MessageAsync(Application.Features.Chat.Dtos.ChatMessageEvent message, IReadOnlyCollection<Guid> userIds,
                             CancellationToken ct) =>
        hub.Clients.Users(userIds.Distinct().Select(id => id.ToString()).ToList())
                   .SendAsync(ChatHub.Events.Message, message, ct);

    /// <inheritdoc />
    public Task ReadAsync(Application.Features.Chat.Dtos.ChatReadEvent read, Guid userId, CancellationToken ct) =>
        hub.Clients.User(userId.ToString()).SendAsync(ChatHub.Events.Read, read, ct);
}
