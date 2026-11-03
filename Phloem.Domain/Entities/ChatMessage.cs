using Phloem.Domain.Enums;
using Phloem.Domain.Rules;

namespace Phloem.Domain.Entities;

/// <summary>One message in an order chat: text, a photo, or both.</summary>
/// <remarks>
/// <para><b>Never edited.</b> "Delete" only hides it for the person who sent it (<see cref="HiddenBySender"/>); the
/// other side and staff still see it, so the record stays complete for disputes.</para>
/// <para><see cref="CreatedAt"/> is when it was sent (server time).</para>
/// </remarks>
public class ChatMessage : Entity<Guid>
{
    /// <summary>Longest message.</summary>
    public const int BodyMaxLength = 1000;

    /// <summary>For EF Core.</summary>
    protected ChatMessage() { }

    private ChatMessage(Guid id) : base(id) { }

    /// <summary>The chat.</summary>
    public Guid ChatId { get; private set; }

    /// <summary>Who sent it.</summary>
    public Guid SenderId { get; private set; }

    /// <summary>Their side.</summary>
    public ChatRole SenderRole { get; private set; }

    /// <summary>The text (may be empty when there's a photo).</summary>
    public string Body { get; private set; } = string.Empty;

    /// <summary>The photo (private storage), if any.</summary>
    public string? PhotoPath { get; private set; }

    /// <summary>The sender chose "Delete" (hidden for them only).</summary>
    public bool HiddenBySender { get; private set; }

    /// <summary>The text looked like it shared contact or payment details (the sender was warned; staff see a mark).</summary>
    public bool HasContactDetails { get; private set; }

    /// <summary>A new message.</summary>
    /// <param name="chat">The chat.</param>
    /// <param name="senderId">Who.</param>
    /// <param name="role">Their side.</param>
    /// <param name="body">The text (trimmed; may be empty with a photo).</param>
    /// <param name="photoPath">The saved photo, if any.</param>
    /// <exception cref="ArgumentException">No text and no photo; text too long.</exception>
    public static ChatMessage Write(OrderChat chat, Guid senderId, ChatRole role, string? body, string? photoPath)
    {
        ArgumentNullException.ThrowIfNull(chat);
        var text = body?.Trim() ?? string.Empty;
        if (text.Length == 0 && photoPath is null) throw new ArgumentException("A message needs text or a photo.", nameof(body));
        if (text.Length > BodyMaxLength) throw new ArgumentException($"At most {BodyMaxLength} characters.", nameof(body));

        return new ChatMessage(Guid.CreateVersion7())
        {
            ChatId = chat.Id,
            SenderId = senderId,
            SenderRole = role,
            Body = text,
            PhotoPath = photoPath,
            HasContactDetails = OffPlatformHints.Found(text)
        };
    }

    /// <summary>"Delete": hidden for the sender only.</summary>
    public void HideForSender() => HiddenBySender = true;
}
