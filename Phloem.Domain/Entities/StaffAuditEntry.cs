using Phloem.Domain.Enums;

namespace Phloem.Domain.Entities;

/// <summary>
/// One line in the staff audit log: who did what, to what, in which country, and when.
/// </summary>
/// <remarks>
/// <para><b>Why:</b> owners can see who opened people's messages and personal details, and every change to who can do
/// what. Later steps add order views, refunds, chats and emails.</para>
/// <para><b>Never changed or deleted</b> by the app. <see cref="Entity{TId}.CreatedAt"/> is when it happened.</para>
/// <para><b>Target</b> is something a person can read ("User 4b0e…", "Parcel PHS-261102-000123"), not just an Id, so
/// the log still makes sense after records change.</para>
/// </remarks>
public class StaffAuditEntry : Entity<Guid>
{
    /// <summary>Longest <see cref="Target"/>.</summary>
    public const int TargetMaxLength = 100;

    /// <summary>Longest <see cref="Detail"/>.</summary>
    public const int DetailMaxLength = 500;

    /// <summary>For EF Core.</summary>
    protected StaffAuditEntry() { }

    private StaffAuditEntry(Guid id) : base(id) { }

    /// <summary>Who did it (staff member or admin).</summary>
    public Guid ActorId { get; private set; }

    /// <summary>What they did.</summary>
    public StaffAction Action { get; private set; }

    /// <summary>What it was done to, e.g. "User 4b0e…" or "Parcel PHS-261102-000123".</summary>
    public string Target { get; private set; } = string.Empty;

    /// <summary>The country of the record, when it has one.</summary>
    public Country? Country { get; private set; }

    /// <summary>A short note, e.g. "Role: ShipmentManager" or "Countries: Ghana, Nigeria".</summary>
    public string? Detail { get; private set; }

    /// <summary>Makes a new log line.</summary>
    /// <param name="actorId">Who did it.</param>
    /// <param name="action">What they did.</param>
    /// <param name="target">What it was done to (cut to <see cref="TargetMaxLength"/>).</param>
    /// <param name="country">The record's country, if any.</param>
    /// <param name="detail">Optional note (cut to <see cref="DetailMaxLength"/>).</param>
    /// <returns>The new line (not yet saved).</returns>
    /// <exception cref="ArgumentException">No actor or no target.</exception>
    /// <exception cref="ArgumentOutOfRangeException">Unknown action or country.</exception>
    public static StaffAuditEntry Create(Guid actorId, StaffAction action, string target,
                                         Country? country = null, string? detail = null)
    {
        if (actorId == Guid.Empty) throw new ArgumentException("Who did it is required.", nameof(actorId));
        if (!Enum.IsDefined(action)) throw new ArgumentOutOfRangeException(nameof(action));
        if (country is { } c && !Enum.IsDefined(c)) throw new ArgumentOutOfRangeException(nameof(country));
        if (string.IsNullOrWhiteSpace(target)) throw new ArgumentException("A target is required.", nameof(target));

        return new StaffAuditEntry(Guid.CreateVersion7())
        {
            ActorId = actorId,
            Action = action,
            Target = Cut(target.Trim(), TargetMaxLength),
            Country = country,
            Detail = string.IsNullOrWhiteSpace(detail) ? null : Cut(detail.Trim(), DetailMaxLength)
        };
    }

    // Logging must never fail because a note is long, so cut instead of refusing
    private static string Cut(string value, int maxLength) => value.Length <= maxLength ? value : value[..maxLength];
}
