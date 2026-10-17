using Phloem.Domain.Enums;

namespace Phloem.Domain.Shared.Errors;

/// <summary>
/// Every business-rule failure for shipments and shipping routes. Nothing here is thrown.
/// </summary>
/// <remarks>ErrorType decides the HTTP status: Validation 400, Forbidden 403, NotFound 404, Conflict 409.</remarks>
public static class ShipmentErrors
{
    // ---------- Finding ----------

    /// <summary>
    /// No shipment with that tracking number, or it isn't the caller's. Both give the same answer,
    /// so nobody can find out about other people's parcels.
    /// </summary>
    public static readonly Error NotFound = Error.NotFound(
        "Shipment.NotFound", "That shipment wasn't found.");

    // ---------- Booking ----------

    /// <summary>The sender's account was deleted.</summary>
    public static readonly Error AccountDeleted = Error.Forbidden(
        "Shipment.AccountDeleted", "This account has been deleted.");

    /// <summary>The route doesn't exist or is switched off.</summary>
    public static readonly Error RouteNotAvailable = Error.Conflict(
        "Shipment.RouteNotAvailable", "We don't ship on this route at the moment.");

    /// <summary>The chosen speed has no price on this route.</summary>
    /// <param name="speed">The chosen speed.</param>
    public static Error SpeedNotOffered(DeliverySpeed speed) => Error.Validation(
        "Shipment.SpeedNotOffered", $"{speed} shipping isn't offered on this route. Please choose another speed.");

    /// <summary>A shipment needs at least one item.</summary>
    public static readonly Error NoItems = Error.Validation(
        "Shipment.NoItems", "Please add at least one item to your parcel.");

    /// <summary>The sender didn't confirm the parcel contains no prohibited items.</summary>
    public static readonly Error MustConfirmNoProhibitedItems = Error.Validation(
        "Shipment.MustConfirmNoProhibitedItems", "Please confirm your parcel contains no prohibited items.");

    // ---------- Paying ----------

    /// <summary>Paying before the parcel has been weighed at the hub.</summary>
    public static readonly Error NotReadyForPayment = Error.Conflict(
        "Shipment.NotReadyForPayment", "You can pay once your parcel has been weighed at our hub.");

    // ---------- After booking ----------

    /// <summary>Cancelling a parcel that has already left.</summary>
    public static readonly Error CannotCancel = Error.Conflict(
        "Shipment.CannotCancel", "This shipment has already left, so it can't be cancelled.");

    /// <summary>A step that skips ahead, e.g. Paid straight to Delivered.</summary>
    /// <param name="from">The current status.</param>
    /// <param name="to">The status that was asked for.</param>
    public static Error InvalidStatusChange(ShipmentStatus from, ShipmentStatus to) => Error.Conflict(
        "Shipment.InvalidStatusChange", $"A shipment that is {from} can't move to {to}.");

    // ---------- Routes (admin) ----------

    /// <summary>Creating a route that already exists (same From and To).</summary>
    public static readonly Error RouteAlreadyExists = Error.Conflict(
        "Shipment.RouteAlreadyExists", "This route already exists.");

    /// <summary>Switching a route on before any speed has a price.</summary>
    public static readonly Error RouteHasNoRates = Error.Validation(
        "Shipment.RouteHasNoRates", "Add a price for at least one speed before switching this route on.");

    /// <summary>The route doesn't exist (admin screens).</summary>
    public static readonly Error RouteNotFound = Error.NotFound(
        "Shipment.RouteNotFound", "That route wasn't found.");
}