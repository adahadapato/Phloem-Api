namespace Phloem.Domain.Shared.Errors;

/// <summary>
/// Business-rule failures for couriers and proof of delivery (food orders and parcels). Nothing here is thrown.
/// </summary>
/// <remarks>ErrorType decides the HTTP status: Validation 400, Forbidden 403, NotFound 404, Conflict 409.</remarks>
public static class DeliveryErrors
{
    // ---------- Choosing a courier ----------

    /// <summary>The chosen person isn't a courier (any more), or their account is suspended or deleted.</summary>
    public static readonly Error CourierNotFound = Error.NotFound(
        "Delivery.CourierNotFound", "That courier wasn't found. Please choose another one.");

    /// <summary>The courier works in a different country from the delivery address.</summary>
    public static readonly Error CourierInAnotherCountry = Error.Validation(
        "Delivery.CourierInAnotherCountry", "That courier doesn't deliver in this country. Please choose another one.");

    /// <summary>Changing the courier of something that isn't out for delivery.</summary>
    public static readonly Error NotOutForDelivery = Error.Conflict(
        "Delivery.NotOutForDelivery", "The courier can only be changed while it's out for delivery.");

    // ---------- Delivering ----------

    /// <summary>
    /// Marking delivered something that isn't assigned to the caller. Same answer as "not found" in the handlers, so
    /// couriers can't learn about other people's deliveries; used by the Domain as a safety net.
    /// </summary>
    public static readonly Error NotYourDelivery = Error.NotFound(
        "Delivery.NotFound", "That delivery wasn't found in your deliveries.");

    /// <summary>Left with a neighbour without saying which one.</summary>
    public static readonly Error SayWhichNeighbour = Error.Validation(
        "Delivery.SayWhichNeighbour", "Please say which neighbour took it, e.g. \"Mrs Ade at No. 14\".");
}
