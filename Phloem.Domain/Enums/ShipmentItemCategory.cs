namespace Phloem.Domain.Enums;

/// <summary>
/// What kind of item is being shipped (step 4 of the "Send a parcel" form). Used for customs.
/// </summary>
/// <remarks>The starting list; add new values at the end as the business grows. Never renumber.</remarks>
public enum ShipmentItemCategory
{
    /// <summary>Sealed / dried / packaged food (gari, spices, dried fish…).</summary>
    PackagedFood = 1,

    /// <summary>Clothes, fabrics, shoes.</summary>
    Clothing = 2,

    /// <summary>Papers, certificates, letters.</summary>
    Documents = 3,

    /// <summary>Phones, laptops, chargers and other electronics.</summary>
    Electronics = 4,

    /// <summary>Anything else (described in the item description).</summary>
    Other = 5
}