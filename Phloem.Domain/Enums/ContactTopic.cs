namespace Phloem.Domain.Enums;

/// <summary>What a Contact us message is about (the form's "Topic"), so the team can sort the inbox.</summary>
public enum ContactTopic
{
    /// <summary>A food order: delivery, refund, cancellation.</summary>
    Order = 1,

    /// <summary>A parcel: price, weighing, customs, tracking.</summary>
    Parcel = 2,

    /// <summary>Selling on Phloem: opening a shop, approval, payouts.</summary>
    Selling = 3,

    /// <summary>My account: signing in, password, details.</summary>
    Account = 4,

    /// <summary>Anything else, including feedback.</summary>
    Other = 5
}
