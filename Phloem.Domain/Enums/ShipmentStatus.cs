namespace Phloem.Domain.Enums;

/// <summary>
/// Where a parcel is. Normal path:
/// <see cref="Booked"/> → <see cref="ReceivedAtHub"/> → <see cref="Paid"/> → <see cref="InTransit"/> →
/// <see cref="ArrivedInDestination"/> → <see cref="OutForDelivery"/> → <see cref="Delivered"/>.
/// </summary>
/// <remarks>
/// Each step is recorded in the shipment's timeline (<see cref="Entities.ShipmentEvent"/>).
/// Numbers are stored in the database: never change them after the first migration; only add new ones.
/// </remarks>
public enum ShipmentStatus
{
    /// <summary>Booked in the app with an ESTIMATED weight and price. Waiting for drop-off.</summary>
    Booked = 1,

    /// <summary>Dropped off and weighed at the hub; the FINAL price is set. Waiting for payment.</summary>
    ReceivedAtHub = 2,

    /// <summary>The final price has been paid. Waiting to leave.</summary>
    Paid = 3,

    /// <summary>On its way between the two countries. Can no longer be cancelled.</summary>
    InTransit = 4,

    /// <summary>Arrived in the destination country (customs / local hub).</summary>
    ArrivedInDestination = 5,

    /// <summary>With the local courier, on its way to the recipient.</summary>
    OutForDelivery = 6,

    /// <summary>Delivered to the recipient. Final state.</summary>
    Delivered = 7,

    /// <summary>Cancelled by the sender before it left. Final state. Needs a refund if it was paid.</summary>
    Cancelled = 8
}