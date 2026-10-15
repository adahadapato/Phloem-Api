using Phloem.Domain.Enums;

namespace Phloem.Domain.Shared.Errors;

/// <summary>
/// Every business-rule failure an Order can return. Nothing here is thrown.
/// The ErrorType decides the HTTP status (Validation 400, Forbidden 403, NotFound 404, Conflict 409).
/// </summary>
public static class OrderErrors
{
    // ---------- Placing an order ----------

    public static readonly Error EmptyBasket = Error.Validation(
        "Order.EmptyBasket", "Your basket is empty.");

    public static readonly Error MixedCurrencies = Error.Validation(
        "Order.MixedCurrencies", "All dishes in one order must be in the same currency.");

    public static readonly Error AccountDeleted = Error.Forbidden(
        "Order.AccountDeleted", "This account has been deleted.");

    public static readonly Error AddressNotYours = Error.Forbidden(
        "Order.AddressNotYours", "That address belongs to another account.");

    /// <summary>A method, because the message names the dish.</summary>
    public static Error DishUnavailable(string dishName) => Error.Conflict(
        "Order.DishUnavailable", $"\"{dishName}\" isn't available right now.");

    // ---------- After placing ----------

    public static readonly Error CancelWindowPassed = Error.Conflict(
        "Order.CancelWindowPassed",
        "This order can no longer be cancelled. The shop has started preparing it.");

    /// <summary>E.g. trying to mark a Placed order as Delivered without preparing / sending it.</summary>
    public static Error InvalidStatusChange(OrderStatus from, OrderStatus to) => Error.Conflict(
        "Order.InvalidStatusChange", $"An order that is {from} can't move to {to}.");
}