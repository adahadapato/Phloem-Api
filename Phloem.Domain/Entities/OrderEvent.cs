using Phloem.Domain.Enums;
using Phloem.Domain.ValueObjects;

namespace Phloem.Domain.Entities;

/// <summary>
/// One step in an order's timeline: what happened, when, who did it, and a short note.
/// </summary>
/// <remarks>
/// <para><b>Written by the order itself</b> (<see cref="Order"/>'s methods add a line whenever they change it), in
/// the same save as the change, so a step can't be missed. Never changed or deleted.</para>
/// <para><b>Rebuilt lines:</b> orders placed before the timeline existed get lines rebuilt from the dates they kept
/// (<see cref="IsRebuilt"/> = true): the time is right, but who did it may be a best guess.</para>
/// </remarks>
public class OrderEvent : Entity<Guid>
{
    /// <summary>Longest <see cref="Note"/>.</summary>
    public const int NoteMaxLength = 500;

    /// <summary>For EF Core.</summary>
    protected OrderEvent() { }

    internal OrderEvent(Guid orderId, OrderEventKind kind, OrderStatus statusAfter, DateTime occurredAtUtc,
                        OrderActor by, string? note, bool isRebuilt = false)
        : base(Guid.CreateVersion7())
    {
        OrderId = orderId;
        Kind = kind;
        StatusAfter = statusAfter;
        OccurredAtUtc = occurredAtUtc;
        ActorKind = by.Kind;
        ActorId = by.UserId;
        var text = note?.Trim();
        Note = string.IsNullOrEmpty(text) ? null : text.Length <= NoteMaxLength ? text : text[..NoteMaxLength];
        IsRebuilt = isRebuilt;
    }

    /// <summary>The order.</summary>
    public Guid OrderId { get; private set; }

    /// <summary>What happened.</summary>
    public OrderEventKind Kind { get; private set; }

    /// <summary>The order's status straight after it.</summary>
    public OrderStatus StatusAfter { get; private set; }

    /// <summary>When it happened (UTC).</summary>
    public DateTime OccurredAtUtc { get; private set; }

    /// <summary>What kind of person did it.</summary>
    public OrderActorKind ActorKind { get; private set; }

    /// <summary>Their account; null when Phloem did it.</summary>
    public Guid? ActorId { get; private set; }

    /// <summary>A short note, e.g. "Courier: Kofi Asante" or the decline reason.</summary>
    public string? Note { get; private set; }

    /// <summary>True for lines rebuilt from an older order's saved dates.</summary>
    public bool IsRebuilt { get; private set; }
}
