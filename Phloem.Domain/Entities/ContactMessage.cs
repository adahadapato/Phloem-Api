using Phloem.Domain.Enums;

namespace Phloem.Domain.Entities;

/// <summary>
/// A message sent with the Contact us form (website or app), kept in the team's inbox (Admin → Contact messages)
/// so nothing is lost if an email goes astray. The team answers by email; here they only mark it Replied or
/// Closed, with an optional note for colleagues.
/// </summary>
/// <remarks>
/// Anyone can write, signed in or not; <see cref="UserId"/> is set when they were signed in. Bad values are
/// checked by the validators first, so they THROW here.
/// </remarks>
public class ContactMessage : Entity<Guid>
{
    /// <summary>Longest name.</summary>
    public const int NameMaxLength = 100;

    /// <summary>Longest email address.</summary>
    public const int EmailMaxLength = 256;

    /// <summary>Longest order or tracking number.</summary>
    public const int ReferenceMaxLength = 40;

    /// <summary>Shortest message (stops "hi").</summary>
    public const int MessageMinLength = 10;

    /// <summary>Longest message.</summary>
    public const int MessageMaxLength = 2000;

    /// <summary>Longest staff note.</summary>
    public const int NoteMaxLength = 500;

    /// <summary>Required by EF Core.</summary>
    protected ContactMessage() { }

    private ContactMessage(Guid id) : base(id) { }

    /// <summary>Who wrote, as they typed it.</summary>
    public string Name { get; private set; } = string.Empty;

    /// <summary>Where to answer.</summary>
    public string Email { get; private set; } = string.Empty;

    /// <summary>What it's about.</summary>
    public ContactTopic Topic { get; private set; }

    /// <summary>An order or tracking number they gave, if any (e.g. "PHL-261024-001").</summary>
    public string? Reference { get; private set; }

    /// <summary>The message itself (plain text).</summary>
    public string Message { get; private set; } = string.Empty;

    /// <summary>The account they were signed in with, or null.</summary>
    public Guid? UserId { get; private set; }

    /// <summary>New, Replied or Closed.</summary>
    public ContactMessageStatus Status { get; private set; } = ContactMessageStatus.New;

    /// <summary>A note for colleagues, e.g. "Refund sent, waiting for the bank".</summary>
    public string? StaffNote { get; private set; }

    /// <summary>When a team member last changed the status.</summary>
    public DateTime? HandledAtUtc { get; private set; }

    /// <summary>The team member who last changed the status.</summary>
    public Guid? HandledByUserId { get; private set; }

    /// <summary>A new message from the Contact us form.</summary>
    /// <param name="name">Not empty, at most <see cref="NameMaxLength"/> characters.</param>
    /// <param name="email">Not empty, at most <see cref="EmailMaxLength"/> characters.</param>
    /// <param name="topic">What it's about.</param>
    /// <param name="reference">Optional order or tracking number.</param>
    /// <param name="message"><see cref="MessageMinLength"/> to <see cref="MessageMaxLength"/> characters.</param>
    /// <param name="userId">The signed-in account, or null.</param>
    /// <returns>The message, status New.</returns>
    /// <exception cref="ArgumentException">An empty name, email or message.</exception>
    /// <exception cref="ArgumentOutOfRangeException">Unknown topic, or text too long or too short.</exception>
    public static ContactMessage Create(string name, string email, ContactTopic topic, string? reference,
                                        string message, Guid? userId)
    {
        if (!Enum.IsDefined(topic)) throw new ArgumentOutOfRangeException(nameof(topic));
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        ArgumentException.ThrowIfNullOrWhiteSpace(email);
        ArgumentException.ThrowIfNullOrWhiteSpace(message);
        ArgumentOutOfRangeException.ThrowIfGreaterThan(name.Trim().Length, NameMaxLength, nameof(name));
        ArgumentOutOfRangeException.ThrowIfGreaterThan(email.Trim().Length, EmailMaxLength, nameof(email));
        ArgumentOutOfRangeException.ThrowIfLessThan(message.Trim().Length, MessageMinLength, nameof(message));
        ArgumentOutOfRangeException.ThrowIfGreaterThan(message.Trim().Length, MessageMaxLength, nameof(message));

        var cleanReference = string.IsNullOrWhiteSpace(reference) ? null : reference.Trim();
        if (cleanReference is not null)
            ArgumentOutOfRangeException.ThrowIfGreaterThan(cleanReference.Length, ReferenceMaxLength, nameof(reference));

        return new ContactMessage(Guid.CreateVersion7())
        {
            Name = name.Trim(),
            Email = email.Trim(),
            Topic = topic,
            Reference = cleanReference,
            Message = message.Trim(),
            UserId = userId
        };
    }

    /// <summary>A team member marks the message (Replied, Closed, or back to New) and may leave a note.</summary>
    /// <param name="status">The new status.</param>
    /// <param name="note">Optional note; null or empty removes it. At most <see cref="NoteMaxLength"/> characters.</param>
    /// <param name="staffUserId">Who did it.</param>
    /// <param name="utcNow">When.</param>
    /// <exception cref="ArgumentOutOfRangeException">Unknown status, or a note that's too long.</exception>
    public void SetStatus(ContactMessageStatus status, string? note, Guid staffUserId, DateTime utcNow)
    {
        if (!Enum.IsDefined(status)) throw new ArgumentOutOfRangeException(nameof(status));
        var cleanNote = string.IsNullOrWhiteSpace(note) ? null : note.Trim();
        if (cleanNote is not null)
            ArgumentOutOfRangeException.ThrowIfGreaterThan(cleanNote.Length, NoteMaxLength, nameof(note));

        Status = status;
        StaffNote = cleanNote;
        HandledAtUtc = utcNow;
        HandledByUserId = staffUserId;
    }
}
