namespace Phloem.Domain.Enums;

/// <summary>
/// Which team inbox receives the reply when someone presses "Reply" on an email from Phloem.
/// Every email names one, so replies always reach the right people.
/// </summary>
/// <remarks>The actual addresses are settings (<c>Email:ReplyTo</c>), so they can change without code.</remarks>
public enum EmailReplyTo
{
    /// <summary>Customers: orders, account, price changes.</summary>
    Support = 1,

    /// <summary>Sellers: shop approval, price ranges, delivery fees, documents.</summary>
    Sellers = 2,

    /// <summary>Parcels: weighing, payment, delivery.</summary>
    Shipping = 3
}