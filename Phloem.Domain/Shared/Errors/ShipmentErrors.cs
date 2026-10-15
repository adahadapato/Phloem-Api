using Phloem.Domain.Enums;

namespace Phloem.Domain.Shared.Errors;

/// <summary>
/// Every business-rule failure a Shipment ("Send food to the UK") can return. Nothing here is thrown.
/// </summary>
public static class ShipmentErrors
{
    public static readonly Error AccountDeleted = Error.Forbidden(
        "Shipment.AccountDeleted", "This account has been deleted.");

    public static readonly Error NotFromUk = Error.Validation(
        "Shipment.NotFromUk", "Shipments are sent from Ghana or Nigeria to the UK.");

    public static readonly Error CannotCancel = Error.Conflict(
        "Shipment.CannotCancel", "This shipment has already left, so it can't be cancelled.");

    /// <summary>E.g. marking a Processing shipment as Delivered without it going in transit.</summary>
    public static Error InvalidStatusChange(ShipmentStatus from, ShipmentStatus to) => Error.Conflict(
        "Shipment.InvalidStatusChange", $"A shipment that is {from} can't move to {to}.");
}
