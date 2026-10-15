namespace Phloem.Domain.Entities;

/// <summary>
/// One dish in an order. Name, shop and price are copied from the Dish when the order is placed,
/// so if the seller renames the dish or changes the price later, this order doesn't change.
/// Only created and changed by its Order.
/// </summary>
public class OrderLine : Entity<Guid>
{
    protected OrderLine() { }

    internal OrderLine(Guid orderId, Dish dish, int quantity)
        : base(Guid.CreateVersion7())
    {
        if (quantity < 1) throw new ArgumentOutOfRangeException(nameof(quantity), "Quantity must be at least 1.");

        OrderId = orderId;
        DishId = dish.Id;
        DishName = dish.Name;
        // Small picture for the order history: thumbnail if there is one, else the full photo
        DishImagePath = dish.PrimaryPhoto?.File.ThumbnailPath ?? dish.PrimaryPhoto?.File.Path;
        VendorId = dish.VendorId;
        VendorName = dish.Vendor.Name;
        UnitPrice = dish.Price;
        Quantity = quantity;
    }

    public Guid OrderId { get; private set; }

    // Kept so "Buy again" can find the dish; the dish may since have been deleted.
    public Guid DishId { get; private set; }

    // ---------- Snapshot at ordering time ----------
    public string DishName { get; private set; } = string.Empty;
    public string? DishImagePath { get; private set; }
    public Guid VendorId { get; private set; }
    public string VendorName { get; private set; } = string.Empty;
    public decimal UnitPrice { get; private set; }

    public int Quantity { get; private set; }
    public decimal LineTotal => UnitPrice * Quantity;

    internal void AddQuantity(int quantity)
    {
        if (quantity < 1) throw new ArgumentOutOfRangeException(nameof(quantity), "Quantity must be at least 1.");
        Quantity += quantity;
    }
}