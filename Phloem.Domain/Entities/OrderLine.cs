namespace Phloem.Domain.Entities;

/// <summary>
/// One dish in an <see cref="Order"/>, with a quantity.
/// </summary>
/// <remarks>
/// <para>
/// The dish's name, picture and price are COPIED ("snapshot") into the line, so later changes
/// to the dish (rename, new photo, deleted) never change an order the customer already has.
/// </para>
/// <para>
/// While the order is unpaid, <see cref="UnitPrice"/> is the price when the dish was added; the
/// app compares it with the dish's current price to show "Price changed: 40.00 → 45.00".
/// When the customer pays, <see cref="Order.PrepareForPayment"/> refreshes the snapshot to
/// today's values, and from then on the line never changes.
/// </para>
/// <para>
/// The picture file is never deleted from storage, even if the seller removes the photo or the
/// dish, so order history always shows what was ordered.
/// </para>
/// Only created and changed through its <see cref="Order"/>.
/// </remarks>
public class OrderLine : Entity<Guid>
{
    /// <summary>Required by EF Core.</summary>
    protected OrderLine() { }

    /// <summary>Creates a line from the dish as it is right now.</summary>
    /// <param name="orderId">The order this line belongs to.</param>
    /// <param name="dish">The dish being ordered (with its photos loaded).</param>
    /// <param name="quantity">How many; at least 1.</param>
    /// <exception cref="ArgumentOutOfRangeException">Quantity below 1 (the validator should have caught it).</exception>
    internal OrderLine(Guid orderId, Dish dish, int quantity)
        : base(Guid.CreateVersion7())
    {
        ArgumentOutOfRangeException.ThrowIfLessThan(quantity, 1);
        ArgumentOutOfRangeException.ThrowIfGreaterThan(quantity, Order.MaxQuantityPerDish);

        OrderId = orderId;
        DishId = dish.Id;
        Quantity = quantity;
        CopyFrom(dish);
    }

    /// <summary>The order this line belongs to.</summary>
    public Guid OrderId { get; private set; }

    /// <summary>
    /// The dish that was ordered. Kept so "Order again" and the out-of-stock check can find it;
    /// the dish may since have been deleted.
    /// </summary>
    public Guid DishId { get; private set; }

    // ---------- Snapshot ----------

    /// <summary>The dish's full name when added (or when paid), e.g. "Jollof Rice &amp; Chicken".</summary>
    public string DishName { get; private set; } = string.Empty;

    /// <summary>Storage path of the dish's picture (thumbnail if there is one). Null if the dish had no photo.</summary>
    public string? DishImagePath { get; private set; }

    /// <summary>
    /// Price per item. Unpaid order: the price when added. Paid order: the price actually paid.
    /// </summary>
    public decimal UnitPrice { get; private set; }

    /// <summary>
    /// The last price the customer was told about (in the weekly email). Starts as the price when
    /// added, so nobody is emailed about the same change twice.
    /// </summary>
    public decimal LastNotifiedPrice { get; private set; }

    /// <summary>How many of this dish.</summary>
    public int Quantity { get; private set; }

    /// <summary><see cref="UnitPrice"/> × <see cref="Quantity"/>.</summary>
    public decimal LineTotal => UnitPrice * Quantity;

    // ---------- Changes (only through Order) ----------

    /// <summary>The same dish added again: the quantities are added together.</summary>
    /// <param name="quantity">How many more; at least 1.</param>
    /// <exception cref="ArgumentOutOfRangeException">Quantity below 1.</exception>
    internal void AddQuantity(int quantity)
    {
        ArgumentOutOfRangeException.ThrowIfLessThan(quantity, 1);
        Quantity += quantity;
    }

    /// <summary>At payment: copy today's name, picture and price from the dish.</summary>
    /// <param name="dish">The same dish, as it is now.</param>
    /// <exception cref="InvalidOperationException">The dish isn't the one this line is for (a bug).</exception>
    internal void RefreshFrom(Dish dish)
    {
        if (dish.Id != DishId)
            throw new InvalidOperationException("Can't refresh an order line from a different dish.");

        CopyFrom(dish);
    }

    /// <summary>Records that the customer has been told about this price (weekly email sent).</summary>
    /// <param name="price">The price that was in the email.</param>
    internal void MarkPriceNotified(decimal price) => LastNotifiedPrice = price;

    /// <summary>Copies the snapshot fields from the dish.</summary>
    private void CopyFrom(Dish dish)
    {
        DishName = dish.Name;
        DishImagePath = dish.PrimaryPhoto?.File.ThumbnailPath ?? dish.PrimaryPhoto?.File.Path;
        UnitPrice = dish.Price;
        LastNotifiedPrice = dish.Price;
    }
}