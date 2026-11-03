using Phloem.Domain.Enums;

namespace Phloem.Domain.Entities;

/// <summary>
/// The conversation about one order, between the customer and the shop. There is no other way to message on
/// Phloem: every chat belongs to a paid order.
/// </summary>
/// <remarks>
/// <para><b>Open</b> from payment until <see cref="OpenDaysAfterOrder"/> days after the order is delivered or cancelled,
/// then read-only. Created with the first message.</para>
/// <para><b>Read</b> markers drive the "seen" ticks and the unread counts; <b>emailed</b> markers make sure each
/// unread batch is emailed once.</para>
/// <para><b>Reported:</b> either side can send it to Phloem's staff. Staff with <c>chats.read</c> read it (audited).</para>
/// </remarks>
public class OrderChat : Entity<Guid>
{
    /// <summary>Days a chat stays open after the order is delivered or cancelled (decided 3 Nov 2026).</summary>
    public const int OpenDaysAfterOrder = 7;

    /// <summary>Longest report reason.</summary>
    public const int ReportReasonMaxLength = 500;

    /// <summary>For EF Core.</summary>
    protected OrderChat() { }

    private OrderChat(Guid id) : base(id) { }

    /// <summary>The order.</summary>
    public Guid OrderId { get; private set; }

    /// <summary>The order number (for lists and links).</summary>
    public string OrderNumber { get; private set; } = string.Empty;

    /// <summary>The customer.</summary>
    public Guid CustomerId { get; private set; }

    /// <summary>The shop.</summary>
    public Guid ShopId { get; private set; }

    /// <summary>The shop's owner (who writes for the shop).</summary>
    public Guid ShopOwnerId { get; private set; }

    /// <summary>The shop's name when the order was placed.</summary>
    public string ShopName { get; private set; } = string.Empty;

    /// <summary>Where the order is delivered (staff see chats in their countries).</summary>
    public Country Country { get; private set; }

    /// <summary>Last message from the customer.</summary>
    public DateTime? LastCustomerMessageAtUtc { get; private set; }

    /// <summary>Last message from the shop.</summary>
    public DateTime? LastSellerMessageAtUtc { get; private set; }

    /// <summary>The customer has seen everything up to here.</summary>
    public DateTime? CustomerReadUpToUtc { get; private set; }

    /// <summary>The shop has seen everything up to here.</summary>
    public DateTime? SellerReadUpToUtc { get; private set; }

    /// <summary>The customer was emailed about messages up to here.</summary>
    public DateTime? CustomerEmailedUpToUtc { get; private set; }

    /// <summary>The shop was emailed about messages up to here.</summary>
    public DateTime? SellerEmailedUpToUtc { get; private set; }

    /// <summary>Sent to Phloem's staff.</summary>
    public bool IsReported { get; private set; }

    /// <summary>When.</summary>
    public DateTime? ReportedAtUtc { get; private set; }

    /// <summary>Who.</summary>
    public Guid? ReportedBy { get; private set; }

    /// <summary>Why.</summary>
    public string? ReportReason { get; private set; }

    /// <summary>A staff member dealt with the report.</summary>
    public DateTime? ReportHandledAtUtc { get; private set; }

    /// <summary>Who dealt with it.</summary>
    public Guid? ReportHandledBy { get; private set; }

    /// <summary>Last message from either side.</summary>
    public DateTime? LastMessageAtUtc =>
        LastCustomerMessageAtUtc > LastSellerMessageAtUtc || LastSellerMessageAtUtc is null
            ? LastCustomerMessageAtUtc
            : LastSellerMessageAtUtc;

    /// <summary>A new chat for an order (with its first message).</summary>
    /// <param name="order">The order.</param>
    /// <param name="shopOwnerId">The shop's owner.</param>
    public static OrderChat Start(Order order, Guid shopOwnerId)
    {
        ArgumentNullException.ThrowIfNull(order);
        if (shopOwnerId == Guid.Empty) throw new ArgumentException("The shop's owner is required.", nameof(shopOwnerId));

        return new OrderChat(Guid.CreateVersion7())
        {
            OrderId = order.Id,
            OrderNumber = order.OrderNumber,
            CustomerId = order.CustomerId,
            ShopId = order.VendorId,
            ShopOwnerId = shopOwnerId,
            ShopName = order.VendorName,
            Country = order.DeliveryCountry
        };
    }

    /// <summary>The person's side in this chat, or null when they aren't in it.</summary>
    /// <param name="userId">The person.</param>
    public ChatRole? RoleOf(Guid userId) =>
        userId == CustomerId ? ChatRole.Customer : userId == ShopOwnerId ? ChatRole.Seller : null;

    /// <summary>The other side's account.</summary>
    /// <param name="role">This side.</param>
    public Guid OtherParty(ChatRole role) => role == ChatRole.Customer ? ShopOwnerId : CustomerId;

    /// <summary>A message was written by this side.</summary>
    /// <param name="role">Who wrote.</param>
    /// <param name="utcNow">When.</param>
    public void MessageWritten(ChatRole role, DateTime utcNow)
    {
        if (role == ChatRole.Customer) { LastCustomerMessageAtUtc = utcNow; CustomerReadUpToUtc = utcNow; }
        else { LastSellerMessageAtUtc = utcNow; SellerReadUpToUtc = utcNow; }
    }

    /// <summary>This side has seen everything so far.</summary>
    /// <returns>True when it changed (so the other side can be told).</returns>
    public bool MarkRead(ChatRole role, DateTime utcNow)
    {
        var incoming = role == ChatRole.Customer ? LastSellerMessageAtUtc : LastCustomerMessageAtUtc;
        var readUpTo = role == ChatRole.Customer ? CustomerReadUpToUtc : SellerReadUpToUtc;
        if (incoming is null || readUpTo >= incoming) return false;

        if (role == ChatRole.Customer) CustomerReadUpToUtc = utcNow; else SellerReadUpToUtc = utcNow;
        return true;
    }

    /// <summary>When this side last saw the chat.</summary>
    public DateTime? ReadUpTo(ChatRole role) => role == ChatRole.Customer ? CustomerReadUpToUtc : SellerReadUpToUtc;

    /// <summary>True when this side has messages they haven't seen.</summary>
    public bool HasUnread(ChatRole role)
    {
        var incoming = role == ChatRole.Customer ? LastSellerMessageAtUtc : LastCustomerMessageAtUtc;
        return incoming is not null && !(ReadUpTo(role) >= incoming);
    }

    /// <summary>True when this side should get an email: unread messages older than <paramref name="cutoffUtc"/>
    /// that weren't emailed yet.</summary>
    public bool NeedsEmail(ChatRole role, DateTime cutoffUtc)
    {
        var incoming = role == ChatRole.Customer ? LastSellerMessageAtUtc : LastCustomerMessageAtUtc;
        var emailed = role == ChatRole.Customer ? CustomerEmailedUpToUtc : SellerEmailedUpToUtc;
        return incoming is not null && incoming <= cutoffUtc && HasUnread(role) && !(emailed >= incoming);
    }

    /// <summary>This side was emailed about everything so far.</summary>
    public void Emailed(ChatRole role, DateTime utcNow)
    {
        if (role == ChatRole.Customer) CustomerEmailedUpToUtc = utcNow; else SellerEmailedUpToUtc = utcNow;
    }

    /// <summary>Sends the chat to Phloem's staff.</summary>
    /// <param name="by">Who reported it.</param>
    /// <param name="reason">Why (cut to <see cref="ReportReasonMaxLength"/>).</param>
    /// <param name="utcNow">When.</param>
    public void Report(Guid by, string reason, DateTime utcNow)
    {
        if (string.IsNullOrWhiteSpace(reason)) throw new ArgumentException("A reason is required.", nameof(reason));
        var clean = reason.Trim();
        IsReported = true;
        ReportedAtUtc = utcNow;
        ReportedBy = by;
        ReportReason = clean.Length <= ReportReasonMaxLength ? clean : clean[..ReportReasonMaxLength];
        ReportHandledAtUtc = null;
        ReportHandledBy = null;
    }

    /// <summary>A staff member dealt with the report.</summary>
    public void CloseReport(Guid staffId, DateTime utcNow)
    {
        ReportHandledAtUtc = utcNow;
        ReportHandledBy = staffId;
    }

    /// <summary>True while the report waits for staff.</summary>
    public bool ReportWaiting => IsReported && ReportHandledAtUtc is null;

    /// <summary>Whether people can still write: open from payment until <see cref="OpenDaysAfterOrder"/> days after
    /// delivery or cancellation.</summary>
    /// <param name="order">The order.</param>
    /// <param name="utcNow">Now.</param>
    public static bool IsOpen(Order order, DateTime utcNow)
    {
        if (order.Status == OrderStatus.AwaitingPayment) return false;
        var ended = order.DeliveredAtUtc ?? order.CancelledAtUtc;
        return ended is null || utcNow < ended.Value.AddDays(OpenDaysAfterOrder);
    }
}
