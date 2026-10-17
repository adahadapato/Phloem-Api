using Phloem.Domain.Enums;

namespace Phloem.Domain.Entities;

/// <summary>
/// One step in a shipment's tracking timeline, e.g. "Received at hub · 17 Oct, 10:42 · Weighed: 7.40 kg".
/// </summary>
/// <remarks>Only created through its Shipment; never changed or deleted (it's a history).</remarks>
public class ShipmentEvent : Entity<Guid>
{
    /// <summary>Longest note on a timeline step.</summary>
    public const int NoteMaxLength = 200;

    /// <summary>Required by EF Core.</summary>
    protected ShipmentEvent() { }

    /// <summary>Records a step.</summary>
    /// <param name="shipmentId">The shipment.</param>
    /// <param name="status">The status the shipment moved to.</param>
    /// <param name="occurredAtUtc">When it happened.</param>
    /// <param name="note">Optional detail, e.g. "Weighed: 7.40 kg".</param>
    internal ShipmentEvent(Guid shipmentId, ShipmentStatus status, DateTime occurredAtUtc, string? note)
        : base(Guid.CreateVersion7())
    {
        var cleanNote = string.IsNullOrWhiteSpace(note) ? null : note.Trim();
        if (cleanNote is not null)
            ArgumentOutOfRangeException.ThrowIfGreaterThan(cleanNote.Length, NoteMaxLength, nameof(note));

        ShipmentId = shipmentId;
        Status = status;
        OccurredAtUtc = occurredAtUtc;
        Note = cleanNote;
    }

    /// <summary>The shipment this step belongs to.</summary>
    public Guid ShipmentId { get; private set; }

    /// <summary>The status reached at this step.</summary>
    public ShipmentStatus Status { get; private set; }

    /// <summary>When it happened (UTC).</summary>
    public DateTime OccurredAtUtc { get; private set; }

    /// <summary>Optional detail shown under the step.</summary>
    public string? Note { get; private set; }
}