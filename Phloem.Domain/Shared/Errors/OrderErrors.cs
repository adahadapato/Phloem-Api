using Phloem.Domain.Enums;

namespace Phloem.Domain.Shared.Errors;

/// <summary>
/// Every business-rule failure an <see cref="Entities.Order"/> can return. Nothing here is thrown.
/// </summary>
/// <remarks>
/// The <see cref="ErrorType"/> of each error decides the HTTP status in the API:
/// Validation → 400, Forbidden → 403, NotFound → 404, Conflict → 409.
/// </remarks>
public static class OrderErrors
{
    // ---------- Finding an order ----------

    /// <summary>
    /// The order doesn't exist, or belongs to someone else. Both give the same answer,
    /// so nobody can find out whether another customer's order number exists.
    /// </summary>
    public static readonly Error NotFound = Error.NotFound(
        "Order.NotFound", "That order wasn't found.");

    // ---------- Placing an order ----------

    /// <summary>The basket (or the part of it for one shop) has no dishes.</summary>
    public static readonly Error EmptyBasket = Error.Validation(
        "Order.EmptyBasket", "Your basket is empty.");

    /// <summary>
    /// One order may only hold dishes from ONE shop. The Application layer splits the basket
    /// by shop before calling Order.Place, so seeing this error means that split was skipped.
    /// </summary>
    public static readonly Error MixedShops = Error.Validation(
        "Order.MixedShops", "One order can only contain dishes from one shop.");

    /// <summary>The customer's account was deleted.</summary>
    public static readonly Error AccountDeleted = Error.Forbidden(
        "Order.AccountDeleted", "This account has been deleted.");

    /// <summary>The chosen delivery address belongs to a different account.</summary>
    public static readonly Error AddressNotYours = Error.Forbidden(
        "Order.AddressNotYours", "That address belongs to another account.");

    /// <summary>A dish in the basket can't be ordered right now (switched off, deleted, shop blocked…).</summary>
    /// <param name="dishName">The dish's name, shown in the message.</param>
    public static Error DishUnavailable(string dishName) => Error.Conflict(
        "Order.DishUnavailable", $"\"{dishName}\" isn't available right now.");

    // ---------- Paying ----------

    /// <summary>Paying, or preparing to pay, an order that isn't waiting for payment.</summary>
    public static readonly Error NotAwaitingPayment = Error.Conflict(
        "Order.NotAwaitingPayment", "This order isn't waiting for payment.");

    /// <summary>Every dish in the order is out of stock, so there's nothing left to pay for.</summary>
    public static readonly Error NothingToPay = Error.Conflict(
        "Order.NothingToPay", "All the dishes in this order are out of stock, so there's nothing to pay for.");

    // ---------- After paying ----------

    /// <summary>More than 30 minutes since payment, or the shop has already started.</summary>
    public static readonly Error CancelWindowPassed = Error.Conflict(
        "Order.CancelWindowPassed",
        "This order can no longer be cancelled. The shop has started preparing it.");

    /// <summary>A status change that skips a step, e.g. Paid straight to Delivered.</summary>
    /// <param name="from">The order's current status.</param>
    /// <param name="to">The status that was asked for.</param>
    public static Error InvalidStatusChange(OrderStatus from, OrderStatus to) => Error.Conflict(
        "Order.InvalidStatusChange", $"An order that is {from} can't move to {to}.");

    /// <summary>The delivery address is in a different country from the shop.</summary>
    public static readonly Error AddressInAnotherCountry = Error.Validation(
        "Order.AddressInAnotherCountry", "This shop doesn't deliver to that country. Please choose another address.");

    /// <summary>Only paid orders the shop hasn't accepted yet can be declined.</summary>
    public static readonly Error CannotDecline = Error.Conflict(
        "Order.CannotDecline", "Only new paid orders can be declined. This one was already accepted, cancelled or isn't paid.");
}
