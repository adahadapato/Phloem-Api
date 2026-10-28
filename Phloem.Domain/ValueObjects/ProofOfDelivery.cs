using Phloem.Domain.Enums;
using Phloem.Domain.Shared.Errors;
using Phloem.Domain.Shared.Results;

namespace Phloem.Domain.ValueObjects;

/// <summary>
/// What the courier recorded when they delivered an order or a parcel: a photo (always), how it was handed over, who
/// took it and an optional note. Shown to the customer on the order or parcel page.
/// </summary>
/// <remarks>
/// <para><b>The photo</b> is in PRIVATE storage (it shows someone's home); <see cref="PhotoPath"/> is only ever turned
/// into a short-lived link for the customer, the shop, the courier and staff. Its location data is removed when it's
/// processed.</para>
/// <para>Stored as columns on the order / shipment (<c>DeliveryProof_PhotoPath</c>…); all null until delivered.</para>
/// <para>EF Core builds it through the private parameterless constructor and fills the properties.</para>
/// </remarks>
public sealed class ProofOfDelivery : ValueObject
{
    /// <summary>Longest storage path (column size).</summary>
    public const int PhotoPathMaxLength = 300;

    /// <summary>Longest "received by", e.g. "Mrs Adeyemi at No. 14".</summary>
    public const int ReceivedByMaxLength = 100;

    /// <summary>Longest note, e.g. "Behind the blue gate, gave the gateman a call".</summary>
    public const int NoteMaxLength = 300;

    /// <summary>Required by EF Core; never call from code.</summary>
    private ProofOfDelivery()
    {
        PhotoPath = string.Empty;
    }

    private ProofOfDelivery(string photoPath, HandoverMethod handover, string? receivedBy, string? note,
                            Guid courierId, DateTime capturedAtUtc)
    {
        PhotoPath = photoPath;
        Handover = handover;
        ReceivedBy = receivedBy;
        Note = note;
        CourierId = courierId;
        CapturedAtUtc = capturedAtUtc;
    }

    /// <summary>Where the photo is in private storage, e.g. "deliveries/2026/10/{id}.webp".</summary>
    public string PhotoPath { get; }

    /// <summary>How it was handed over.</summary>
    public HandoverMethod Handover { get; }

    /// <summary>Who took it (required for <see cref="HandoverMethod.Neighbour"/>); null when not given.</summary>
    public string? ReceivedBy { get; }

    /// <summary>The courier's note; null when blank.</summary>
    public string? Note { get; }

    /// <summary>The courier who delivered it (their user Id).</summary>
    public Guid CourierId { get; }

    /// <summary>When the courier confirmed the delivery.</summary>
    public DateTime CapturedAtUtc { get; }

    /// <summary>Checks and tidies what the courier entered (blank text becomes null).</summary>
    /// <param name="photoPath">Where the processed photo was saved (private container).</param>
    /// <param name="handover">How it was handed over.</param>
    /// <param name="receivedBy">Who took it; required for <see cref="HandoverMethod.Neighbour"/>.</param>
    /// <param name="note">An optional note.</param>
    /// <param name="courierId">The courier.</param>
    /// <param name="utcNow">The current time.</param>
    /// <returns>The proof; or <see cref="DeliveryErrors.SayWhichNeighbour"/>.</returns>
    /// <exception cref="ArgumentException">Blank photo path or empty courier Id.</exception>
    /// <exception cref="ArgumentOutOfRangeException">Unknown handover method, or text too long.</exception>
    public static Result<ProofOfDelivery> Create(string photoPath, HandoverMethod handover, string? receivedBy,
                                                 string? note, Guid courierId, DateTime utcNow)
    {
        // Programmer mistakes → throw (the validator and handler guarantee these)
        ArgumentException.ThrowIfNullOrWhiteSpace(photoPath);
        if (courierId == Guid.Empty) throw new ArgumentException("A courier is required.", nameof(courierId));
        if (!Enum.IsDefined(handover)) throw new ArgumentOutOfRangeException(nameof(handover));

        var path = photoPath.Trim();
        var who = string.IsNullOrWhiteSpace(receivedBy) ? null : receivedBy.Trim();
        var text = string.IsNullOrWhiteSpace(note) ? null : note.Trim();

        ArgumentOutOfRangeException.ThrowIfGreaterThan(path.Length, PhotoPathMaxLength, nameof(photoPath));
        ArgumentOutOfRangeException.ThrowIfGreaterThan(who?.Length ?? 0, ReceivedByMaxLength, nameof(receivedBy));
        ArgumentOutOfRangeException.ThrowIfGreaterThan(text?.Length ?? 0, NoteMaxLength, nameof(note));

        // Business rule → return an error
        if (handover == HandoverMethod.Neighbour && who is null) return DeliveryErrors.SayWhichNeighbour;

        return new ProofOfDelivery(path, handover, who, text, courierId, utcNow);
    }

    /// <summary>
    /// A short line for timelines and emails: "Left at the front door", "Left with a neighbour (No. 14)".
    /// </summary>
    public string Describe() => Handover switch
    {
        HandoverMethod.InPerson => ReceivedBy is null ? "Handed over in person" : $"Handed to {ReceivedBy}",
        HandoverMethod.FrontDoor => "Left at the front door",
        HandoverMethod.Neighbour => $"Left with a neighbour ({ReceivedBy})",
        HandoverMethod.SafePlace => "Left in a safe place",
        HandoverMethod.Reception => ReceivedBy is null ? "Left at reception" : $"Left at reception with {ReceivedBy}",
        _ => "Delivered"
    };

    /// <inheritdoc />
    protected override IEnumerable<object?> GetAtomicValues()
    {
        yield return PhotoPath;
        yield return Handover;
        yield return ReceivedBy;
        yield return Note;
        yield return CourierId;
        yield return CapturedAtUtc;
    }
}
