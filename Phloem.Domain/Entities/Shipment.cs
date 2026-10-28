using Phloem.Domain.Enums;
using Phloem.Domain.Models;
using Phloem.Domain.Shared.Errors;
using Phloem.Domain.Shared.Results;
using Phloem.Domain.ValueObjects;

namespace Phloem.Domain.Entities;

/// <summary>
/// A parcel sent between two countries on a <see cref="ShipmentRoute"/> (e.g. Ghana → UK or UK → Nigeria),
/// containing food and/or other items.
/// </summary>
/// <remarks>
/// <para><b>Who:</b> the sender must be a registered, signed-in user (taken from the login, never from
/// the form). The recipient doesn't need an account.</para>
///
/// <para><b>Life cycle</b> (each step is added to the <see cref="Timeline"/>):
/// <list type="number">
///   <item><see cref="Book"/>: ESTIMATED weight and price.</item>
///   <item><see cref="ReceiveAtHub"/>: staff weigh it; the FINAL price is set from the real weight.</item>
///   <item><see cref="MarkPaid"/>: the sender pays the final price.</item>
///   <item><see cref="MarkInTransit"/> → <see cref="MarkArrivedInDestination"/> →
///   <see cref="MarkOutForDelivery"/> (ShipmentManager staff, who choose a courier) → <see cref="MarkDelivered"/>
///   (that courier, with a photo: <see cref="DeliveryProof"/>).</item>
/// </list></para>
///
/// <para><b>Cancelling:</b> allowed until it's in transit. If it was paid, <see cref="NeedsRefund"/>.</para>
///
/// <para><b>Money</b> is in the ORIGIN country's currency (where the sender drops off and pays),
/// including the declared values of the items.</para>
///
/// <para><b>Errors:</b> business-rule failures are RETURNED as a <see cref="Result"/> (see
/// <see cref="ShipmentErrors"/>); programmer mistakes (nulls, blank text, impossible weights) THROW.</para>
/// </remarks>
public class Shipment : Entity<Guid>
{
    /// <summary>Lightest parcel accepted (kg).</summary>
    public const decimal MinWeightKg = 0.1m;

    /// <summary>Heaviest parcel accepted (kg), including Bulk.</summary>
    public const decimal MaxWeightKg = 1000m;

    /// <summary>Most items in one shipment.</summary>
    public const int MaxItems = 50;

    /// <summary>Longest recipient name.</summary>
    public const int RecipientNameMaxLength = 100;

    /// <summary>Longest street line.</summary>
    public const int AddressLineMaxLength = 300;

    /// <summary>Longest city / town.</summary>
    public const int CityMaxLength = 100;

    /// <summary>Longest region / postcode.</summary>
    public const int RegionOrPostcodeMaxLength = 50;

    /// <summary>Longest tracking number (column size). Real ones are 17 characters: "PHS-yyMMdd-######".</summary>
    public const int TrackingNumberMaxLength = 20;

    /// <summary>Longest payment-provider reference (column size).</summary>
    public const int PaymentReferenceMaxLength = 100;

    private readonly List<ShipmentItem> _items = new();
    private readonly List<ShipmentEvent> _timeline = new();

    /// <summary>Required by EF Core.</summary>
    protected Shipment() { }

    // ---------- Identity ----------

    /// <summary>Shown to the sender and recipient; used for public tracking. Unique.</summary>
    public string TrackingNumber { get; private set; } = string.Empty;

    // ---------- Sender (registered user) ----------

    /// <summary>The sender's account (same Id as their UserProfile).</summary>
    public Guid SenderId { get; private set; }

    /// <summary>The sender's profile (navigation, for EF Core).</summary>
    public virtual UserProfile Sender { get; protected set; } = null!;

    /// <summary>Snapshot of the sender's name at booking.</summary>
    public string SenderName { get; private set; } = string.Empty;

    /// <summary>The sender's contact phone for this parcel.</summary>
    public PhoneNumber SenderPhone { get; private set; } = null!;

    // ---------- Route ----------

    /// <summary>The route booked on.</summary>
    public Guid ShipmentRouteId { get; private set; }

    /// <summary>Origin country (snapshot of the route).</summary>
    public Country FromCountry { get; private set; }

    /// <summary>Destination country (snapshot of the route).</summary>
    public Country ToCountry { get; private set; }

    // ---------- Recipient ----------

    /// <summary>The recipient's full name.</summary>
    public string RecipientName { get; private set; } = string.Empty;

    /// <summary>The recipient's phone.</summary>
    public PhoneNumber RecipientPhone { get; private set; } = null!;

    /// <summary>Street and house number.</summary>
    public string RecipientAddressLine { get; private set; } = string.Empty;

    /// <summary>City or town.</summary>
    public string RecipientCity { get; private set; } = string.Empty;

    /// <summary>Region or postcode.</summary>
    public string RecipientRegionOrPostcode { get; private set; } = string.Empty;

    // ---------- Parcel ----------

    /// <summary>The items declared by the sender.</summary>
    public virtual IReadOnlyCollection<ShipmentItem> Items => _items.AsReadOnly();

    /// <summary>Sum of the items' declared values (for customs).</summary>
    public decimal TotalDeclaredValue => _items.Sum(i => i.TotalDeclaredValue);

    /// <summary>The chosen speed.</summary>
    public DeliverySpeed Speed { get; private set; }

    /// <summary>The weight the sender entered when booking (kg).</summary>
    public decimal EstimatedWeightKg { get; private set; }

    /// <summary>The weight measured at the hub (kg); null until weighed.</summary>
    public decimal? WeighedWeightKg { get; private set; }

    // ---------- Money (origin currency) ----------

    /// <summary>The origin country's currency.</summary>
    public Currency Currency { get; private set; }

    /// <summary>Price from the estimated weight, shown when booking.</summary>
    public decimal EstimatedPrice { get; private set; }

    /// <summary>Price from the weighed weight: what the sender pays. Null until weighed.</summary>
    public decimal? FinalPrice { get; private set; }

    /// <summary>Fewest transit days for this parcel (copied from the rate when weighed). Null until weighed.</summary>
    public int? TransitDaysMin { get; private set; }

    /// <summary>Most transit days for this parcel (copied from the rate when weighed). Null until weighed.</summary>
    public int? TransitDaysMax { get; private set; }

    /// <summary>
    /// "Expected 22–24 Oct": the day it left + the transit days. Null until it's in transit
    /// (or if no transit days are known). Once delivered, the app shows the delivery date instead.
    /// </summary>
    public (DateOnly From, DateOnly To)? EstimatedArrival
    {
        get
        {
            var left = _timeline.FirstOrDefault(e => e.Status == ShipmentStatus.InTransit);
            if (left is null || TransitDaysMin is null || TransitDaysMax is null) return null;

            var day = DateOnly.FromDateTime(left.OccurredAtUtc);
            return (day.AddDays(TransitDaysMin.Value), day.AddDays(TransitDaysMax.Value));
        }
    }


    /// <summary>The payment provider's reference; null until paid.</summary>
    public string? PaymentReference { get; private set; }

    // ---------- Status ----------

    /// <summary>Where the parcel is.</summary>
    public ShipmentStatus Status { get; private set; }

    /// <summary>When the sender confirmed the parcel contains no prohibited items (at booking).</summary>
    public DateTime NoProhibitedItemsConfirmedAtUtc { get; private set; }

    /// <summary>When it was paid; null until then.</summary>
    public DateTime? PaidAtUtc { get; private set; }

    /// <summary>When it was delivered; null until then.</summary>
    public DateTime? DeliveredAtUtc { get; private set; }

    /// <summary>When it was cancelled; null if not cancelled.</summary>
    public DateTime? CancelledAtUtc { get; private set; }

    // ---------- Delivery (courier) ----------

    /// <summary>The courier delivering it in the destination country; null until it's out for delivery.</summary>
    public Guid? CourierId { get; private set; }

    /// <summary>Snapshot of the courier's name; null until it's out for delivery.</summary>
    public string? CourierName { get; private set; }

    /// <summary>When the current courier was given the parcel; null until it's out for delivery.</summary>
    public DateTime? CourierAssignedAtUtc { get; private set; }

    /// <summary>The photo and handover details recorded on delivery; null until delivered.</summary>
    public ProofOfDelivery? DeliveryProof { get; private set; }

    /// <summary>The tracking timeline, oldest first.</summary>
    public virtual IReadOnlyCollection<ShipmentEvent> Timeline => _timeline.OrderBy(e => e.OccurredAtUtc).ToList();

    /// <summary>True once paid.</summary>
    public bool IsPaid => PaidAtUtc is not null;

    /// <summary>A paid shipment that was cancelled: the sender must get their money back.</summary>
    public bool NeedsRefund => Status == ShipmentStatus.Cancelled && IsPaid;

    /// <summary>Can the sender still cancel? Only before it leaves (Booked, ReceivedAtHub or Paid).</summary>
    public bool CanCancel => Status is ShipmentStatus.Booked or ShipmentStatus.ReceivedAtHub or ShipmentStatus.Paid;

    // =====================================================================
    // Booking
    // =====================================================================

    /// <summary>
    /// "Send a parcel" → Book. Creates the shipment with an ESTIMATED price from the estimated weight.
    /// </summary>
    /// <param name="trackingNumber">A new, unique tracking number (made by the Application layer).</param>
    /// <param name="sender">The signed-in sender.</param>
    /// <param name="senderPhone">The sender's contact phone for this parcel.</param>
    /// <param name="route">The route (with its rates loaded).</param>
    /// <param name="recipient">Who receives it and where (in the route's destination country).</param>
    /// <param name="items">At least one item, at most <see cref="MaxItems"/>.</param>
    /// <param name="speed">The chosen speed.</param>
    /// <param name="estimatedWeightKg"><see cref="MinWeightKg"/> to <see cref="MaxWeightKg"/>.</param>
    /// <param name="confirmedNoProhibitedItems">The sender ticked "My parcel contains no prohibited items".</param>
    /// <param name="utcNow">The current time.</param>
    /// <returns>
    /// The shipment; or <see cref="ShipmentErrors.AccountDeleted"/>, <see cref="ShipmentErrors.MustConfirmNoProhibitedItems"/>,
    /// <see cref="ShipmentErrors.NoItems"/>, <see cref="ShipmentErrors.RouteNotAvailable"/>,
    /// <see cref="ShipmentErrors.SpeedNotOffered"/>.
    /// </returns>
    /// <exception cref="ArgumentNullException">A null argument.</exception>
    /// <exception cref="ArgumentException">Blank tracking number or recipient text.</exception>
    /// <exception cref="ArgumentOutOfRangeException">Weight outside the limits, too many items, or text too long.</exception>
    public static Result<Shipment> Book(string trackingNumber, UserProfile sender, PhoneNumber senderPhone,
                                        ShipmentRoute route, ShipmentRecipient recipient,
                                        IEnumerable<ShipmentItemDetails> items, DeliverySpeed speed,
                                        decimal estimatedWeightKg, bool confirmedNoProhibitedItems, DateTime utcNow)
    {
        // Programmer mistakes → throw (validators check these first)
        ArgumentException.ThrowIfNullOrWhiteSpace(trackingNumber);
        ArgumentNullException.ThrowIfNull(sender);
        ArgumentNullException.ThrowIfNull(senderPhone);
        ArgumentNullException.ThrowIfNull(route);
        ArgumentNullException.ThrowIfNull(recipient);
        ArgumentNullException.ThrowIfNull(items);
        CheckWeight(estimatedWeightKg);
        CheckRecipient(recipient);

        var itemList = items.ToList();
        ArgumentOutOfRangeException.ThrowIfGreaterThan(itemList.Count, MaxItems, nameof(items));

        // Business rules → return an error
        if (sender.IsDeleted) return ShipmentErrors.AccountDeleted;
        if (!confirmedNoProhibitedItems) return ShipmentErrors.MustConfirmNoProhibitedItems;
        if (itemList.Count == 0) return ShipmentErrors.NoItems;

        var price = route.PriceFor(speed, estimatedWeightKg);   // route active + speed offered
        if (price.IsFailure) return price.Error;

        var shipment = new Shipment
        {
            TrackingNumber = trackingNumber.Trim(),
            SenderId = sender.Id,
            SenderName = sender.GetFullName(),
            SenderPhone = senderPhone,
            ShipmentRouteId = route.Id,
            FromCountry = route.FromCountry,
            ToCountry = route.ToCountry,
            RecipientName = recipient.FullName.Trim(),
            RecipientPhone = recipient.Phone,
            RecipientAddressLine = recipient.AddressLine.Trim(),
            RecipientCity = recipient.City.Trim(),
            RecipientRegionOrPostcode = recipient.RegionOrPostcode.Trim(),
            Speed = speed,
            EstimatedWeightKg = estimatedWeightKg,
            Currency = route.Currency,
            EstimatedPrice = price.Value,
            Status = ShipmentStatus.Booked,
            NoProhibitedItemsConfirmedAtUtc = utcNow
        };
        shipment.Id = Guid.CreateVersion7();

        foreach (var item in itemList)
            shipment._items.Add(new ShipmentItem(shipment.Id, item));

        shipment.AddEvent(ShipmentStatus.Booked, utcNow, note: null);
        return shipment;
    }

    // =====================================================================
    // Hub and payment
    // =====================================================================

    /// <summary>
    /// Staff at the origin hub received and weighed the parcel. Sets the FINAL price from the real
    /// weight, using the route's rates as they are today.
    /// </summary>
    /// <param name="route">This shipment's route (with its rates loaded).</param>
    /// <param name="weighedWeightKg">The measured weight, <see cref="MinWeightKg"/> to <see cref="MaxWeightKg"/>.</param>
    /// <param name="utcNow">The current time.</param>
    /// <returns>Success; or <see cref="ShipmentErrors.InvalidStatusChange"/>, <see cref="ShipmentErrors.SpeedNotOffered"/>.</returns>
    /// <exception cref="InvalidOperationException">The route isn't this shipment's route (a bug).</exception>
    /// <exception cref="ArgumentOutOfRangeException">Weight outside the limits.</exception>
    public Result ReceiveAtHub(ShipmentRoute route, decimal weighedWeightKg, DateTime utcNow)
    {
        ArgumentNullException.ThrowIfNull(route);
        if (route.Id != ShipmentRouteId)
            throw new InvalidOperationException("This isn't the shipment's route.");
        CheckWeight(weighedWeightKg);

        if (Status != ShipmentStatus.Booked)
            return ShipmentErrors.InvalidStatusChange(Status, ShipmentStatus.ReceivedAtHub);

        // Priced even if the route was switched off since booking: the parcel is already at the hub
        var rate = route.Rates.FirstOrDefault(r => r.Speed == Speed);
        if (rate is null) return ShipmentErrors.SpeedNotOffered(Speed);

        WeighedWeightKg = weighedWeightKg;
        FinalPrice = decimal.Round(rate.BaseCharge + rate.PricePerKg * ShipmentRoute.ChargeableWeight(weighedWeightKg),
                                   2, MidpointRounding.AwayFromZero);
        TransitDaysMin = rate.MinTransitDays;
        TransitDaysMax = rate.MaxTransitDays;
        Status = ShipmentStatus.ReceivedAtHub;
        AddEvent(Status, utcNow, $"Weighed: {weighedWeightKg:0.00} kg");
        return Result.Success();
    }

    /// <summary>The payment provider confirmed the FINAL price was paid.</summary>
    /// <param name="paymentReference">The provider's reference.</param>
    /// <param name="utcNow">The current time.</param>
    /// <returns>
    /// Success (also when called again with the SAME reference, as payment callbacks often repeat);
    /// or <see cref="ShipmentErrors.NotReadyForPayment"/> if it hasn't been weighed yet.
    /// </returns>
    /// <exception cref="ArgumentException">Blank payment reference.</exception>
    public Result MarkPaid(string paymentReference, DateTime utcNow)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(paymentReference);
        var reference = paymentReference.Trim();

        if (IsPaid && PaymentReference == reference) return Result.Success();
        if (Status != ShipmentStatus.ReceivedAtHub) return ShipmentErrors.NotReadyForPayment;

        PaymentReference = reference;
        PaidAtUtc = utcNow;
        Status = ShipmentStatus.Paid;
        AddEvent(Status, utcNow, note: null);
        return Result.Success();
    }

    // =====================================================================
    // Journey (ShipmentManager staff)
    // =====================================================================

    /// <summary>The parcel has left the origin country (Paid → InTransit). No cancelling after this.</summary>
    /// <param name="utcNow">The current time.</param>
    /// <param name="note">Optional detail, e.g. "Flight BA078".</param>
    /// <returns>Success; or <see cref="ShipmentErrors.InvalidStatusChange"/>.</returns>
    public Result MarkInTransit(DateTime utcNow, string? note = null) =>
        MoveTo(ShipmentStatus.InTransit, ShipmentStatus.Paid, utcNow, note);

    /// <summary>Arrived in the destination country (InTransit → ArrivedInDestination).</summary>
    /// <param name="utcNow">The current time.</param>
    /// <param name="note">Optional detail, e.g. "Cleared customs".</param>
    /// <returns>Success; or <see cref="ShipmentErrors.InvalidStatusChange"/>.</returns>
    public Result MarkArrivedInDestination(DateTime utcNow, string? note = null) =>
        MoveTo(ShipmentStatus.ArrivedInDestination, ShipmentStatus.InTransit, utcNow, note);

    /// <summary>With a local courier (ArrivedInDestination → OutForDelivery).</summary>
    /// <param name="courierId">The courier (the Application layer checked the role and country).</param>
    /// <param name="courierName">Their name, kept as a snapshot.</param>
    /// <param name="utcNow">The current time.</param>
    /// <param name="note">Optional detail; the timeline says "With our courier, {name}" when blank.</param>
    /// <returns>Success; or <see cref="ShipmentErrors.InvalidStatusChange"/>.</returns>
    /// <exception cref="ArgumentException">Empty courier Id or blank name.</exception>
    public Result MarkOutForDelivery(Guid courierId, string courierName, DateTime utcNow, string? note = null)
    {
        CheckCourier(courierId, courierName);

        var text = string.IsNullOrWhiteSpace(note) ? CourierNote(courierName) : note;
        var moved = MoveTo(ShipmentStatus.OutForDelivery, ShipmentStatus.ArrivedInDestination, utcNow, text);
        if (moved.IsFailure) return moved;

        SetCourier(courierId, courierName, utcNow);
        return Result.Success();
    }

    /// <summary>
    /// Gives a parcel that's out for delivery to another courier (recorded in the timeline). The same courier again
    /// changes nothing.
    /// </summary>
    /// <param name="courierId">The new courier.</param>
    /// <param name="courierName">Their name.</param>
    /// <param name="utcNow">The current time.</param>
    /// <returns>Success; or <see cref="DeliveryErrors.NotOutForDelivery"/>.</returns>
    /// <exception cref="ArgumentException">Empty courier Id or blank name.</exception>
    public Result ChangeCourier(Guid courierId, string courierName, DateTime utcNow)
    {
        CheckCourier(courierId, courierName);
        if (Status != ShipmentStatus.OutForDelivery) return DeliveryErrors.NotOutForDelivery;
        if (CourierId == courierId) return Result.Success();

        SetCourier(courierId, courierName, utcNow);
        AddEvent(Status, utcNow, CourierNote(courierName));
        return Result.Success();
    }

    /// <summary>The courier delivered it, with a photo (OutForDelivery → Delivered).</summary>
    /// <param name="proof">The photo and handover details, taken by THIS parcel's courier.</param>
    /// <returns>
    /// Success; or <see cref="DeliveryErrors.NotYourDelivery"/> (someone other than the courier),
    /// <see cref="ShipmentErrors.InvalidStatusChange"/>.
    /// </returns>
    /// <remarks>The timeline step says how it was handed over, e.g. "Left at the front door".</remarks>
    /// <exception cref="ArgumentNullException">No proof: a photo is always required.</exception>
    public Result MarkDelivered(ProofOfDelivery proof)
    {
        ArgumentNullException.ThrowIfNull(proof);
        if (CourierId != proof.CourierId) return DeliveryErrors.NotYourDelivery;

        var moved = MoveTo(ShipmentStatus.Delivered, ShipmentStatus.OutForDelivery, proof.CapturedAtUtc, proof.Describe());
        if (moved.IsFailure) return moved;

        DeliveryProof = proof;
        DeliveredAtUtc = proof.CapturedAtUtc;
        return Result.Success();
    }

    // =====================================================================
    // Cancelling
    // =====================================================================

    /// <summary>The sender cancels before the parcel leaves. Cancelling twice is fine.</summary>
    /// <param name="utcNow">The current time.</param>
    /// <returns>Success; or <see cref="ShipmentErrors.CannotCancel"/>.</returns>
    /// <remarks>If it was paid, <see cref="NeedsRefund"/> becomes true; the Payments feature refunds it.</remarks>
    public Result Cancel(DateTime utcNow)
    {
        if (Status == ShipmentStatus.Cancelled) return Result.Success();
        if (!CanCancel) return ShipmentErrors.CannotCancel;

        Status = ShipmentStatus.Cancelled;
        CancelledAtUtc = utcNow;
        AddEvent(Status, utcNow, note: null);
        return Result.Success();
    }

    // =====================================================================
    // Helpers
    // =====================================================================

    /// <summary>Moves one step forward, only from the expected status, and records it in the timeline.</summary>
    private Result MoveTo(ShipmentStatus to, ShipmentStatus from, DateTime utcNow, string? note)
    {
        if (Status != from) return ShipmentErrors.InvalidStatusChange(Status, to);

        Status = to;
        AddEvent(to, utcNow, note);
        return Result.Success();
    }

    private void AddEvent(ShipmentStatus status, DateTime utcNow, string? note) =>
        _timeline.Add(new ShipmentEvent(Id, status, utcNow, note));

    /// <exception cref="ArgumentException">Empty courier Id or blank name.</exception>
    private static void CheckCourier(Guid courierId, string courierName)
    {
        if (courierId == Guid.Empty) throw new ArgumentException("A courier is required.", nameof(courierId));
        ArgumentException.ThrowIfNullOrWhiteSpace(courierName);
    }

    private void SetCourier(Guid courierId, string courierName, DateTime utcNow)
    {
        var name = courierName.Trim();
        CourierId = courierId;
        CourierName = name.Length <= UserProfile.FullNameMaxLength ? name : name[..UserProfile.FullNameMaxLength];
        CourierAssignedAtUtc = utcNow;
    }

    /// <summary>"With our courier, Kwame Asante" (cut to fit a timeline note).</summary>
    private static string CourierNote(string courierName)
    {
        var note = $"With our courier, {courierName.Trim()}";
        return note.Length <= ShipmentEvent.NoteMaxLength ? note : note[..ShipmentEvent.NoteMaxLength];
    }

    /// <exception cref="ArgumentOutOfRangeException">Weight outside <see cref="MinWeightKg"/>–<see cref="MaxWeightKg"/>.</exception>
    private static void CheckWeight(decimal weightKg)
    {
        ArgumentOutOfRangeException.ThrowIfLessThan(weightKg, MinWeightKg);
        ArgumentOutOfRangeException.ThrowIfGreaterThan(weightKg, MaxWeightKg);
    }

    /// <exception cref="ArgumentException">Blank text.</exception>
    /// <exception cref="ArgumentOutOfRangeException">Text too long.</exception>
    private static void CheckRecipient(ShipmentRecipient r)
    {
        ArgumentNullException.ThrowIfNull(r.Phone);
        ArgumentException.ThrowIfNullOrWhiteSpace(r.FullName);
        ArgumentException.ThrowIfNullOrWhiteSpace(r.AddressLine);
        ArgumentException.ThrowIfNullOrWhiteSpace(r.City);
        ArgumentException.ThrowIfNullOrWhiteSpace(r.RegionOrPostcode);
        ArgumentOutOfRangeException.ThrowIfGreaterThan(r.FullName.Trim().Length, RecipientNameMaxLength, nameof(r));
        ArgumentOutOfRangeException.ThrowIfGreaterThan(r.AddressLine.Trim().Length, AddressLineMaxLength, nameof(r));
        ArgumentOutOfRangeException.ThrowIfGreaterThan(r.City.Trim().Length, CityMaxLength, nameof(r));
        ArgumentOutOfRangeException.ThrowIfGreaterThan(r.RegionOrPostcode.Trim().Length, RegionOrPostcodeMaxLength, nameof(r));
    }
}