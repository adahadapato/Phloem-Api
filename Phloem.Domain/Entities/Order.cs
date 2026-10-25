using Phloem.Domain.Enums;
using Phloem.Domain.Models;
using Phloem.Domain.Shared.Errors;
using Phloem.Domain.Shared.Results;

namespace Phloem.Domain.Entities;

/// <summary>
/// A customer's order from ONE shop.
/// </summary>
/// <remarks>
/// <para><b>One shop per order.</b> A basket with dishes from two shops becomes two orders (the
/// Application layer splits it), because each shop prepares, sends and gets paid on its own.</para>
///
/// <para><b>Life cycle.</b>
/// <list type="number">
///   <item><see cref="Place"/>: the order is saved as <see cref="OrderStatus.AwaitingPayment"/>.
///   It is the customer's saved basket: it never expires, and sellers can't see it.</item>
///   <item>While unpaid, the app shows today's price for each line and marks dishes that can't be
///   ordered any more as "Out of stock" (worked out live; nothing is stored).</item>
///   <item><see cref="PrepareForPayment"/>: "Pay" tapped. Prices are refreshed to today's and
///   out-of-stock lines are dropped. The total is now what the customer will be charged.</item>
///   <item><see cref="MarkPaid"/>: the payment provider confirmed. The seller now sees the order.</item>
///   <item><see cref="StartPreparing"/> → <see cref="SendOut"/> → <see cref="MarkDelivered"/>.</item>
/// </list></para>
///
/// <para><b>Cancelling.</b> Unpaid: any time. Paid: within <see cref="CancelWindow"/> of payment,
/// and only before the shop starts preparing. A paid order that is cancelled needs a refund
/// (<see cref="NeedsRefund"/>).</para>
///
/// <para><b>Snapshots.</b> The delivery address, shop name and each dish's name, picture and price
/// are copied in, so later edits never change an order the customer already has.</para>
///
/// <para><b>Errors.</b> Business-rule failures are RETURNED as a <see cref="Result"/> (see
/// <see cref="OrderErrors"/>); programmer mistakes (null arguments, blank order number, negative
/// fee) THROW.</para>
/// </remarks>
public class Order : Entity<Guid>
{
    /// <summary>How long after PAYMENT the customer can still cancel (if the shop hasn't started).</summary>
    public static readonly TimeSpan CancelWindow = TimeSpan.FromMinutes(30);

    /// <summary>Most of one dish in a single order (stops typos like 1000 plates).</summary>
    public const int MaxQuantityPerDish = 99;

    /// <summary>Longest order number (column size). Real ones are 16 characters: "PH-yyMMdd-######".</summary>
    public const int OrderNumberMaxLength = 20;

    /// <summary>Longest payment-provider reference (column size); Paystack and Stripe references are far shorter.</summary>
    public const int PaymentReferenceMaxLength = 100;

    /// <summary>Longest reason a shop can give for declining (the buyer sees it).</summary>
    public const int DeclineReasonMaxLength = 500;

    private readonly List<OrderLine> _lines = new();

    /// <summary>Required by EF Core.</summary>
    protected Order() { }

    private Order(Guid id, string orderNumber, Guid customerId, Vendor vendor, decimal deliveryFee,
               DeliveryWindow deliveryWindow, DeliveryAddress address, DateTime placedAtUtc)
     : base(id)
    {
        OrderNumber = orderNumber;
        CustomerId = customerId;
        VendorId = vendor.Id;
        VendorName = vendor.Name;
        Currency = vendor.Currency;
        DeliveryFee = deliveryFee;
        DeliveryWindow = deliveryWindow;
        DeliveryLabel = address.Label;
        DeliveryAddressLine = address.FullAddress;
        DeliveryCountry = address.Country;
        DeliveryRecipientName = address.RecipientName;
        DeliveryPhone = address.PhoneNumber?.Value;
        PlacedAtUtc = placedAtUtc;
        Status = OrderStatus.AwaitingPayment;
    }

    // ---------- Who ----------

    /// <summary>Shown to customers and sellers, e.g. "PH-240518-4821". Unique.</summary>
    public string OrderNumber { get; private set; } = string.Empty;

    /// <summary>The customer (same Id as their UserProfile and login account).</summary>
    public Guid CustomerId { get; private set; }

    /// <summary>The customer's profile (navigation, for EF Core).</summary>
    public virtual UserProfile Customer { get; protected set; } = null!;

    /// <summary>The ONE shop this order is from.</summary>
    public Guid VendorId { get; private set; }

    /// <summary>Snapshot of the shop's name when the order was placed.</summary>
    public string VendorName { get; private set; } = string.Empty;

    // ---------- Where (snapshot) ----------

    /// <summary>Snapshot of the address label, e.g. "Home".</summary>
    public string DeliveryLabel { get; private set; } = string.Empty;

    /// <summary>Snapshot of the full address, e.g. "12 Ring Road, Accra".</summary>
    public string DeliveryAddressLine { get; private set; } = string.Empty;

    /// <summary>Country of the delivery address.</summary>
    public Country DeliveryCountry { get; private set; }

    /// <summary>Snapshot of who receives it (the address's name); null = the customer themselves.</summary>
    public string? DeliveryRecipientName { get; private set; }

    /// <summary>Snapshot of the number the seller calls (the address's phone, e.g. "+447700900123"); null = the
    /// customer's own phone.</summary>
    public string? DeliveryPhone { get; private set; }

    /// <summary>When the customer wants it (ASAP / this evening / tomorrow). Affects the delivery fee.</summary>
    public DeliveryWindow DeliveryWindow { get; private set; }

    // ---------- Money ----------

    /// <summary>The shop's currency; every line is in it.</summary>
    public Currency Currency { get; private set; }

    /// <summary>
    /// Sum of the lines. Unpaid: based on the prices when added (the app shows a live total).
    /// Paid: what was actually charged for the dishes.
    /// </summary>
    public decimal Subtotal { get; private set; }

    /// <summary>Delivery fee, worked out by the Application layer (refreshed at payment).</summary>
    public decimal DeliveryFee { get; private set; }

    /// <summary><see cref="Subtotal"/> + <see cref="DeliveryFee"/>.</summary>
    public decimal Total => Subtotal + DeliveryFee;

    // ---------- Status ----------

    /// <summary>Where the order is in its life (see <see cref="OrderStatus"/>).</summary>
    public OrderStatus Status { get; private set; }

    /// <summary>When the customer placed the order (unpaid).</summary>
    public DateTime PlacedAtUtc { get; private set; }

    /// <summary>When payment was confirmed; null while unpaid.</summary>
    public DateTime? PaidAtUtc { get; private set; }

    /// <summary>The payment provider's reference (Paystack / Stripe), for refunds and support.</summary>
    public string? PaymentReference { get; private set; }

    /// <summary>When the order was cancelled; null if not cancelled.</summary>
    public DateTime? CancelledAtUtc { get; private set; }

    /// <summary>True when the SHOP declined it (rather than the buyer cancelling).</summary>
    public bool DeclinedByShop { get; private set; }

    /// <summary>Why the shop declined it (the buyer sees it); null otherwise.</summary>
    public string? DeclineReason { get; private set; }

    /// <summary>When the order was delivered; null until then.</summary>
    public DateTime? DeliveredAtUtc { get; private set; }

    /// <summary>
    /// What Phloem kept from the dishes, worked out when the order was delivered (snapshot).
    /// Null until delivered.
    /// </summary>
    public decimal? CommissionAmount { get; private set; }

    /// <summary>
    /// What was added to the seller's balance: <see cref="Total"/> − <see cref="CommissionAmount"/>
    /// (the delivery fee always goes to the seller). Null until delivered.
    /// </summary>
    public decimal? SellerEarnings { get; private set; }

    /// <summary>True once payment has been confirmed.</summary>
    public bool IsPaid => PaidAtUtc is not null;

    /// <summary>A paid order that was cancelled: the customer must get their money back.</summary>
    public bool NeedsRefund => Status == OrderStatus.Cancelled && IsPaid;

    /// <summary>The dishes in this order.</summary>
    public virtual IReadOnlyCollection<OrderLine> Lines => _lines.AsReadOnly();

    // =====================================================================
    // Placing
    // =====================================================================

    /// <summary>
    /// Places an UNPAID order for one shop's dishes from the customer's basket.
    /// </summary>
    /// <param name="orderNumber">A new, unique order number (made by the Application layer).</param>
    /// <param name="customer">The signed-in customer.</param>
    /// <param name="address">One of the customer's saved delivery addresses.</param>
    /// <param name="items">Dishes (with their Vendor and photos loaded) and quantities, all from ONE shop.</param>
    /// <param name="deliveryFee">Delivery fee for this shop's order (from DeliveryPricing.FeeFor); 0 or more.</param>
    /// <param name="deliveryWindow">The delivery window the customer chose.</param>
    /// <param name="utcNow">The current time (from TimeProvider).</param>
    /// <returns>
    /// The new order, or: <see cref="OrderErrors.AccountDeleted"/>, <see cref="OrderErrors.AddressNotYours"/>,
    /// <see cref="OrderErrors.EmptyBasket"/>, <see cref="OrderErrors.MixedShops"/>,
    /// <see cref="OrderErrors.DishUnavailable"/>, <see cref="VendorErrors.ClosedForToday"/>.
    /// </returns>
    /// <exception cref="ArgumentException">Blank order number.</exception>
    /// <exception cref="ArgumentNullException">A null argument.</exception>
    /// <exception cref="ArgumentOutOfRangeException">Negative fee, or a quantity below 1.</exception>
    public static Result<Order> Place(string orderNumber, UserProfile customer, DeliveryAddress address,
                                      IEnumerable<(Dish Dish, int Quantity)> items, decimal deliveryFee,
                                      DeliveryWindow deliveryWindow, DateTime utcNow)
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

        var vendor = list[0].Dish.Vendor;
        if (list.Any(i => i.Dish.VendorId != vendor.Id)) return OrderErrors.MixedShops;

        var unavailable = list.FirstOrDefault(i => !i.Dish.CanBeOrdered).Dish;
        if (unavailable is not null) return OrderErrors.DishUnavailable(unavailable.Name);

        // After the shop's cut-off, only "Tomorrow" (shop's local time)
        var open = vendor.CheckDeliveryWindow(deliveryWindow, utcNow);
        if (open.IsFailure) return open.Error;

        var order = new Order(Guid.CreateVersion7(), orderNumber.Trim(), customer.Id, vendor,
                              deliveryFee, deliveryWindow, address, utcNow);

        foreach (var (dish, quantity) in list)
            order.AddLine(dish, quantity);

        return order;
    }

    // =====================================================================
    // Paying
    // =====================================================================

    /// <summary>
    /// "Pay" tapped: refreshes every line to today's name, picture and price, and DROPS lines whose
    /// dish can't be ordered any more ("pay for the rest"). Afterwards <see cref="Total"/> is the
    /// amount to charge.
    /// </summary>
    /// <param name="currentDishes">
    /// The order's dishes as they are now (with Vendor and photos). Dishes that were deleted are
    /// simply missing, and their lines are dropped.
    /// </param>
    /// <param name="deliveryFee">Today's delivery fee for this order.</param>
    /// <returns>
    /// The names of the dishes that were dropped (empty if none), so the app can say
    /// "2 items were out of stock and removed from this payment"; or
    /// <see cref="OrderErrors.NotAwaitingPayment"/>, or <see cref="OrderErrors.NothingToPay"/>
    /// if every dish is out of stock (in that case nothing is changed).
    /// </returns>
    /// <remarks>
    /// Dropped lines are removed for good: the customer was told before paying. If they then don't
    /// complete the payment, the order stays unpaid with the remaining dishes.
    /// </remarks>
    /// <exception cref="ArgumentNullException">currentDishes is null.</exception>
    /// <exception cref="ArgumentOutOfRangeException">Negative fee.</exception>
    public Result<IReadOnlyList<string>> PrepareForPayment(IReadOnlyCollection<Dish> currentDishes, decimal deliveryFee)
    {
        ArgumentNullException.ThrowIfNull(currentDishes);
        ArgumentOutOfRangeException.ThrowIfNegative(deliveryFee);

        if (Status != OrderStatus.AwaitingPayment) return OrderErrors.NotAwaitingPayment;

        var byId = currentDishes.ToDictionary(d => d.Id);
        var outOfStock = _lines
            .Where(line => !byId.TryGetValue(line.DishId, out var dish) || !dish.CanBeOrdered)
            .ToList();

        // Check first, so nothing changes if there's nothing to pay for
        if (outOfStock.Count == _lines.Count) return OrderErrors.NothingToPay;

        foreach (var line in outOfStock)
            _lines.Remove(line);

        foreach (var line in _lines)
            line.RefreshFrom(byId[line.DishId]);

        DeliveryFee = deliveryFee;
        RecalculateSubtotal();

        IReadOnlyList<string> dropped = outOfStock.Select(l => l.DishName).ToList();
        return Result<IReadOnlyList<string>>.Success(dropped);
    }

    /// <summary>
    /// The payment provider confirmed the payment. The seller can now see the order.
    /// </summary>
    /// <param name="paymentReference">The provider's reference for this payment.</param>
    /// <param name="utcNow">The current time.</param>
    /// <returns>
    /// Success; or <see cref="OrderErrors.NotAwaitingPayment"/>. Calling it again with the SAME
    /// reference succeeds and changes nothing, because payment callbacks are often sent twice.
    /// </returns>
    /// <exception cref="ArgumentException">Blank payment reference.</exception>
    public Result MarkPaid(string paymentReference, DateTime utcNow)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(paymentReference);
        var reference = paymentReference.Trim();

        if (Status == OrderStatus.Paid && PaymentReference == reference) return Result.Success();
        if (Status != OrderStatus.AwaitingPayment) return OrderErrors.NotAwaitingPayment;

        Status = OrderStatus.Paid;
        PaidAtUtc = utcNow;
        PaymentReference = reference;
        return Result.Success();
    }

    // =====================================================================
    // Weekly price-change email
    // =====================================================================

    /// <summary>
    /// For the weekly email: finds lines whose dish price changed since the customer was last told,
    /// marks them as told, and returns the changes. Only for unpaid orders; out-of-stock dishes are
    /// skipped (the app already shows them as "Out of stock").
    /// </summary>
    /// <param name="currentDishes">The order's dishes as they are now.</param>
    /// <returns>The changes to put in the email; empty if there are none (or the order is paid).</returns>
    /// <remarks>
    /// Save the order only AFTER the email was sent, so a failed email is retried next week.
    /// A price that went up and back down again since the last email is not a change.
    /// </remarks>
    /// <exception cref="ArgumentNullException">currentDishes is null.</exception>
    public IReadOnlyList<OrderLinePriceChange> TakeUnnotifiedPriceChanges(IReadOnlyCollection<Dish> currentDishes)
    {
        ArgumentNullException.ThrowIfNull(currentDishes);
        if (Status != OrderStatus.AwaitingPayment) return [];

        var byId = currentDishes.ToDictionary(d => d.Id);
        var changes = new List<OrderLinePriceChange>();

        foreach (var line in _lines)
        {
            if (!byId.TryGetValue(line.DishId, out var dish) || !dish.CanBeOrdered) continue;
            if (dish.Price == line.LastNotifiedPrice) continue;

            changes.Add(new OrderLinePriceChange(OrderNumber, line.DishName, line.LastNotifiedPrice, dish.Price, Currency));
            line.MarkPriceNotified(dish.Price);
        }

        return changes;
    }

    // =====================================================================
    // Cancelling
    // =====================================================================

    /// <summary>
    /// Can the customer cancel right now? Unpaid: always. Paid: within <see cref="CancelWindow"/>
    /// of payment and before the shop starts preparing. Otherwise: no.
    /// </summary>
    /// <param name="utcNow">The current time.</param>
    public bool CanCancel(DateTime utcNow) => Status switch
    {
        OrderStatus.AwaitingPayment => true,
        OrderStatus.Paid => utcNow - PaidAtUtc!.Value <= CancelWindow,
        _ => false
    };

    /// <summary>
    /// Time left to cancel a paid order (for a countdown in the app).
    /// Null for an unpaid order (no deadline); zero when it can't be cancelled any more.
    /// </summary>
    /// <param name="utcNow">The current time.</param>
    public TimeSpan? TimeLeftToCancel(DateTime utcNow) => Status switch
    {
        OrderStatus.AwaitingPayment => null,
        OrderStatus.Paid when CanCancel(utcNow) => CancelWindow - (utcNow - PaidAtUtc!.Value),
        _ => TimeSpan.Zero
    };

    /// <summary>Cancels the order. Cancelling twice is fine.</summary>
    /// <param name="utcNow">The current time.</param>
    /// <returns>Success; or <see cref="OrderErrors.CancelWindowPassed"/>.</returns>
    /// <remarks>If the order was paid, <see cref="NeedsRefund"/> becomes true; the Payments feature refunds it.</remarks>
    public Result Cancel(DateTime utcNow)
    {
        if (Status == OrderStatus.Cancelled) return Result.Success();
        if (!CanCancel(utcNow)) return OrderErrors.CancelWindowPassed;

        Status = OrderStatus.Cancelled;
        CancelledAtUtc = utcNow;
        return Result.Success();
    }

    /// <summary>
    /// The shop's owner was suspended, so the shop can't make this order: it's cancelled whatever the buyer's cancel
    /// window says, if it hasn't left the shop yet (awaiting payment, paid or being prepared). A paid one then
    /// <see cref="NeedsRefund"/>. Orders already out for delivery, delivered or cancelled are left alone.
    /// </summary>
    /// <param name="utcNow">The current time.</param>
    /// <returns>True when this call cancelled the order.</returns>
    public bool CancelBecauseShopSuspended(DateTime utcNow)
    {
        if (Status is not (OrderStatus.AwaitingPayment or OrderStatus.Paid or OrderStatus.Preparing)) return false;

        Status = OrderStatus.Cancelled;
        CancelledAtUtc = utcNow;
        return true;
    }

    /// <summary>
    /// The shop declines a paid order it hasn't accepted yet (e.g. an ingredient ran out): the order is cancelled and,
    /// being paid, <see cref="NeedsRefund"/>. The buyer is told the reason.
    /// </summary>
    /// <param name="reason">Why (shown to the buyer).</param>
    /// <param name="utcNow">The current time.</param>
    /// <returns>Success; or <see cref="OrderErrors.CannotDecline"/> (not paid, or already accepted).</returns>
    /// <exception cref="ArgumentException">Blank reason.</exception>
    /// <exception cref="ArgumentOutOfRangeException">Reason longer than <see cref="DeclineReasonMaxLength"/>.</exception>
    public Result DeclineByShop(string reason, DateTime utcNow)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(reason);
        var text = reason.Trim();
        ArgumentOutOfRangeException.ThrowIfGreaterThan(text.Length, DeclineReasonMaxLength, nameof(reason));

        if (Status != OrderStatus.Paid) return OrderErrors.CannotDecline;

        Status = OrderStatus.Cancelled;
        CancelledAtUtc = utcNow;
        DeclinedByShop = true;
        DeclineReason = text;
        return Result.Success();
    }

    // =====================================================================
    // Progress (shop / delivery)
    // =====================================================================

    /// <summary>The seller accepts a paid order and starts cooking (Paid → Preparing).</summary>
    /// <returns>Success; or <see cref="OrderErrors.InvalidStatusChange"/>.</returns>
    public Result StartPreparing() => MoveTo(OrderStatus.Preparing, from: OrderStatus.Paid);

    /// <summary>The order leaves the kitchen (Preparing → OutForDelivery).</summary>
    /// <returns>Success; or <see cref="OrderErrors.InvalidStatusChange"/>.</returns>
    public Result SendOut() => MoveTo(OrderStatus.OutForDelivery, from: OrderStatus.Preparing);


    /// <summary>
    /// The order arrived (OutForDelivery → Delivered). Works out and stores the commission and the
    /// seller's earnings; the Application layer then credits <see cref="SellerEarnings"/> to the shop.
    /// </summary>
    /// <param name="utcNow">The current time.</param>
    /// <param name="commission">The country's rate; null = no rate set (0%).</param>
    /// <returns>Success; or <see cref="OrderErrors.InvalidStatusChange"/>.</returns>
    /// <exception cref="InvalidOperationException">The rate is for a different country (a bug).</exception>
    public Result MarkDelivered(DateTime utcNow, Commission? commission)
    {
        if (commission is not null && commission.Country != DeliveryCountry)
            throw new InvalidOperationException($"This commission is for {commission.Country}, not {DeliveryCountry}.");

        var moved = MoveTo(OrderStatus.Delivered, from: OrderStatus.OutForDelivery);
        if (moved.IsFailure) return moved;

        DeliveredAtUtc = utcNow;
        CommissionAmount = commission?.AmountOn(Subtotal) ?? 0m;
        SellerEarnings = Total - CommissionAmount;
        return Result.Success();
    }

    // =====================================================================
    // Helpers
    // =====================================================================

    /// <summary>Adds a line (or adds to an existing line for the same dish) and updates the subtotal.</summary>
    private void AddLine(Dish dish, int quantity)
    {
        var existing = _lines.FirstOrDefault(l => l.DishId == dish.Id);
        if (existing is not null) existing.AddQuantity(quantity);
        else _lines.Add(new OrderLine(Id, dish, quantity));

        RecalculateSubtotal();
    }

    private void RecalculateSubtotal() => Subtotal = _lines.Sum(l => l.LineTotal);

    /// <summary>Moves one step forward, only from the expected status.</summary>
    private Result MoveTo(OrderStatus to, OrderStatus from)
    {
        if (Status != from) return OrderErrors.InvalidStatusChange(Status, to);
        Status = to;
        return Result.Success();
    }
}