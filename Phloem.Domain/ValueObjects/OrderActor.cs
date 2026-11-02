using Phloem.Domain.Enums;

namespace Phloem.Domain.ValueObjects;

/// <summary>Who did a step in an order's timeline: what kind of person, and which account.</summary>
/// <param name="Kind">Customer, seller, courier, staff or Phloem itself.</param>
/// <param name="UserId">Their account; null for <see cref="OrderActorKind.System"/>.</param>
/// <remarks>Names are looked up when the timeline is shown, so a renamed account shows its current name.</remarks>
public readonly record struct OrderActor(OrderActorKind Kind, Guid? UserId)
{
    /// <summary>The customer.</summary>
    public static OrderActor Customer(Guid userId) => new(OrderActorKind.Customer, Required(userId));

    /// <summary>The shop's owner.</summary>
    public static OrderActor Seller(Guid userId) => new(OrderActorKind.Seller, Required(userId));

    /// <summary>The courier.</summary>
    public static OrderActor Courier(Guid userId) => new(OrderActorKind.Courier, Required(userId));

    /// <summary>A Phloem owner or staff member.</summary>
    public static OrderActor Staff(Guid userId) => new(OrderActorKind.Staff, Required(userId));

    /// <summary>Phloem itself.</summary>
    public static OrderActor System { get; } = new(OrderActorKind.System, null);

    private static Guid Required(Guid userId) =>
        userId == Guid.Empty ? throw new ArgumentException("A person is required.", nameof(userId)) : userId;
}
