namespace Phloem.Domain.Enums;

/// <summary>Who did a step in an order's timeline. Numbers are stored: never renumber.</summary>
public enum OrderActorKind
{
    /// <summary>The person who placed the order.</summary>
    Customer = 1,

    /// <summary>The shop's owner.</summary>
    Seller = 2,

    /// <summary>The person delivering it (may be the seller).</summary>
    Courier = 3,

    /// <summary>A Phloem owner or staff member.</summary>
    Staff = 4,

    /// <summary>Phloem itself (e.g. a payment confirmation, an automatic step).</summary>
    System = 5
}
