namespace Phloem.Domain.Enums;

/// <summary>Where someone accepted the Terms (or was shown the Privacy Policy), kept with each acceptance.</summary>
/// <remarks>The numbers are stored in the database: only add new values at the end.</remarks>
public enum AcceptanceContext
{
    /// <summary>Ticked "I agree…" when creating an account.</summary>
    SignUp = 1,

    /// <summary>Ticked "I agree to the Seller terms" when opening a shop.</summary>
    OpenShop = 2,

    /// <summary>Ticked "I accept the shipping terms" when booking a parcel (reference = tracking number).</summary>
    SendParcel = 3,

    /// <summary>Placed an order under "By placing your order you agree…" (reference = order number).</summary>
    Checkout = 4,

    /// <summary>Pressed Accept on "We've updated our Terms".</summary>
    UpdatedTerms = 5,

    /// <summary>Pressed "Got it" on "We've updated our Privacy Policy" (informed, not consent).</summary>
    PrivacyNotice = 6
}
