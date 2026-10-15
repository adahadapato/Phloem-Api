using Phloem.Domain.Enums;
using Phloem.Domain.Shared.Errors;
using Phloem.Domain.Shared.Results;

namespace Phloem.Domain.Entities;

/// <summary>
/// A customer's order. It keeps a copy ("snapshot") of the delivery address and of each
/// dish's name and price at the moment of ordering, so later edits never change old orders.
///
/// Errors: business-rule failures are RETURNED as a Result (see OrderErrors);
/// programmer mistakes (null arguments, blank order number) still THROW.
/// </summary>
public class Order : Entity<Guid>
{
    /// <summary>How long after placing an order the customer can still cancel it.</summary>
    public static readonly TimeSpan CancelWindow = TimeSpan.FromMinutes(30);

    private readonly List<OrderLine> _lines = new();

    protected Order() { }

    private Order(Guid id, string orderNumber, Guid customerId, Currency currency, decimal deliveryFee,
                  string deliveryLabel, string deliveryAddress, Country deliveryCountry, DateTime placedAtUtc)
        : base(id)
    {
        OrderNumber = orderNumber;
        CustomerId = customerId;
        Currency = currency;
        DeliveryFee = deliveryFee;
        DeliveryLabel = deliveryLabel;
        DeliveryAddressLine = deliveryAddress;
        DeliveryCountry = deliveryCountry;
        PlacedAtUtc = placedAtUtc;
        Status = OrderStatus.Placed;
    }

    // ---------- Who and where ----------
    public string OrderNumber { get; private set; } = string.Empty;  // "PH-240518-4821", shown to customers
    public Guid CustomerId { get; private set; }
    public virtual UserProfile Customer { get; protected set; } = null!;

    public string DeliveryLabel { get; private set; } = string.Empty;       // snapshot: "Home"
    public string DeliveryAddressLine { get; private set; } = string.Empty; // snapshot: "12 Ring Road, Accra"
    public Country DeliveryCountry { get; private set; }

    // ---------- Money ----------
    public Currency Currency { get; private set; }
    public decimal Subtotal { get; private set; }
    public decimal DeliveryFee { get; private set; }
    public decimal Total => Subtotal + DeliveryFee;

    // ---------- Status ----------
    public OrderStatus Status { get; private set; }
    public DateTime PlacedAtUtc { get; private set; }
    public DateTime? CancelledAtUtc { get; private set; }
    public DateTime? DeliveredAtUtc { get; private set; }

    public virtual IReadOnlyCollection<OrderLine> Lines => _lines.AsReadOnly();

    // =====================================================================
    // Behaviour
    // =====================================================================

    /// <summary>
    /// Place an order from the basket. All dishes must be orderable and in the same currency.
    /// The order number and delivery fee are worked out by the Application layer.
    /// </summary>
    public static Result<Order> Place(string orderNumber, UserProfile customer, DeliveryAddress address,
                                      IEnumerable<(Dish Dish, int Quantity)> items, decimal deliveryFee,
                                      DateTime utcNow)
    {
        // Programmer mistakes → throw
        ArgumentException.ThrowIfNullOrWhiteSpace(orderNumber);
        ArgumentNullException.ThrowIfNull(customer);
        ArgumentNullException.ThrowIfNull(address);
        ArgumentNullException.ThrowIfNull(items);
        ArgumentOutOfRangeException.ThrowIfNegative(deliveryFee);

        // Business rules → return an error
        if (customer.IsDeleted) return OrderErrors.AccountDeleted;
        if (address.UserId != customer.Id) return OrderErrors.AddressNotYours;

        var list = items.ToList();
        if (list.Count == 0) return OrderErrors.EmptyBasket;

        var currency = list[0].Dish.Currency;
        if (list.Any(i => i.Dish.Currency != currency)) return OrderErrors.MixedCurrencies;

        var unavailable = list.FirstOrDefault(i => !i.Dish.CanBeOrdered).Dish;
        if (unavailable is not null) return OrderErrors.DishUnavailable(unavailable.Name);

        var order = new Order(Guid.CreateVersion7(), orderNumber.Trim(), customer.Id,
                              currency, deliveryFee, address.Label, address.FullAddress, address.Country, utcNow);

        foreach (var (dish, quantity) in list)
            order.AddLine(dish, quantity);

        return order;   // implicit Order → Result<Order>
    }

    /// <summary>All checks were done in Place, so this can't fail for business reasons.</summary>
    private void AddLine(Dish dish, int quantity)
    {
        // Same dish twice → one line with the quantities added together
        var existing = _lines.FirstOrDefault(l => l.DishId == dish.Id);
        if (existing is not null) existing.AddQuantity(quantity);
        else _lines.Add(new OrderLine(Id, dish, quantity));

        Subtotal = _lines.Sum(l => l.LineTotal);
    }

    // ---------- Cancel ----------
    public bool CanCancel(DateTime utcNow) =>
        Status == OrderStatus.Placed && utcNow - PlacedAtUtc <= CancelWindow;

    public TimeSpan TimeLeftToCancel(DateTime utcNow) =>
        CanCancel(utcNow) ? CancelWindow - (utcNow - PlacedAtUtc) : TimeSpan.Zero;

    public Result Cancel(DateTime utcNow)
    {
        if (Status == OrderStatus.Cancelled) return Result.Success();   // tapping Cancel twice is fine
        if (!CanCancel(utcNow)) return OrderErrors.CancelWindowPassed;

        Status = OrderStatus.Cancelled;
        CancelledAtUtc = utcNow;
        return Result.Success();
    }

    // ---------- Progress (shop / delivery) ----------
    public Result StartPreparing() => MoveTo(OrderStatus.Preparing, from: OrderStatus.Placed);
    public Result SendOut() => MoveTo(OrderStatus.OutForDelivery, from: OrderStatus.Preparing);

    public Result MarkDelivered(DateTime utcNow)
    {
        var moved = MoveTo(OrderStatus.Delivered, from: OrderStatus.OutForDelivery);
        if (moved.IsSuccess) DeliveredAtUtc = utcNow;
        return moved;
    }

    private Result MoveTo(OrderStatus to, OrderStatus from)
    {
        if (Status != from) return OrderErrors.InvalidStatusChange(Status, to);
        Status = to;
        return Result.Success();
    }
}