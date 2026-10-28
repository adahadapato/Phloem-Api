namespace Phloem.Domain.Enums;

/// <summary>
/// How a delivery was handed over, chosen by the courier on the "Delivered" screen and shown to the customer next to
/// the photo ("Left at the front door").
/// </summary>
/// <remarks>Numbers are stored in the database: never change them after the first migration; only add new ones.</remarks>
public enum HandoverMethod
{
    /// <summary>Handed to the customer or recipient in person.</summary>
    InPerson = 1,

    /// <summary>Left at the front door.</summary>
    FrontDoor = 2,

    /// <summary>Left with a neighbour (the courier says which one).</summary>
    Neighbour = 3,

    /// <summary>Left in a safe place, e.g. a porch or behind the gate.</summary>
    SafePlace = 4,

    /// <summary>Left with a reception desk, concierge or security gate (estates, offices, flats).</summary>
    Reception = 5
}
