using Phloem.Application.Features.Chat.Dtos;

namespace Phloem.Application.Features.Chat.Common;

/// <summary>
/// Tells people's open pages about chat changes straight away (implemented in Presentation with SignalR). If nobody
/// is connected nothing happens: pages load the chat when opened, and unread messages are emailed.
/// </summary>
public interface IChatRealtime
{
    /// <summary>A new message, to these people (the sender too, for their other tabs and phone).</summary>
    Task MessageAsync(ChatMessageEvent message, IReadOnlyCollection<Guid> userIds, CancellationToken ct);

    /// <summary>One side has seen the messages, to the other side.</summary>
    Task ReadAsync(ChatReadEvent read, Guid userId, CancellationToken ct);
}
