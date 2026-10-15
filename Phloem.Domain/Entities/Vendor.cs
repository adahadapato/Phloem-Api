using Phloem.Domain.Enums;
using Phloem.Domain.ValueObjects;

namespace Phloem.Domain.Entities;

/// <summary>
/// A seller's shop. A user becomes a seller by owning one.
/// New shops wait for admin approval before buyers can see them.
/// </summary>
public class Vendor : Entity<Guid>
{
    protected Vendor() { }

    private Vendor(Guid id, Guid ownerId, string name, EmailAddress businessEmail,
                   PhoneNumber? businessPhone, Country country)
        : base(id)
    {
        OwnerId = ownerId;
        Name = name;
        BusinessEmail = businessEmail;
        BusinessPhone = businessPhone;
        Country = country;
        Status = VendorStatus.PendingApproval;
    }

    // ---------- Owner ----------
    public Guid OwnerId { get; private set; }
    public virtual UserProfile Owner { get; protected set; } = null!;

    // ---------- Business details (Shop settings page) ----------
    public string Name { get; private set; } = string.Empty;       // unique
    public EmailAddress BusinessEmail { get; private set; } = null!;
    public PhoneNumber? BusinessPhone { get; private set; }
    public Country Country { get; private set; }
    public Currency Currency => Country.ToCurrency();

    // ---------- Status ----------
    public VendorStatus Status { get; private set; }
    public bool IsVerified { get; private set; }                    // the ✓ badge
    public bool IsVisibleToBuyers => Status == VendorStatus.Approved;

    // ---------- Reputation & money ----------
    public decimal Rating { get; private set; }                     // average stars, 0–5
    public int RatingCount { get; private set; }
    public decimal Balance { get; private set; }                    // in this shop's Currency

    // ---------- Dates (CreatedAt / UpdatedAt come from the base Entity) ----------
    public DateTime? ApprovedAtUtc { get; private set; }

    // =====================================================================
    // Behaviour
    // =====================================================================

    /// <summary>
    /// Seller setup. The shop's country is the owner's country.
    /// If the seller leaves business email / phone blank, the Application layer passes in
    /// the login account's email / phone (they live in Identity, not in UserProfile).
    /// </summary>
    public static Vendor Open(UserProfile owner, string name, EmailAddress businessEmail, PhoneNumber? businessPhone)
    {
        ArgumentNullException.ThrowIfNull(owner);
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        ArgumentNullException.ThrowIfNull(businessEmail);
        if (owner.IsDeleted) throw new InvalidOperationException("This account has been deleted.");

        return new Vendor(Guid.CreateVersion7(), owner.Id, name.Trim(),
            businessEmail, businessPhone, owner.Country);
    }

    /// <summary>Shop settings page. Blank email / phone are filled in by the Application layer (as in Open).</summary>
    public void UpdateDetails(string name, EmailAddress businessEmail, PhoneNumber? businessPhone)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        ArgumentNullException.ThrowIfNull(businessEmail);

        Name = name.Trim();
        BusinessEmail = businessEmail;
        BusinessPhone = businessPhone;
    }

    // ---------- Admin actions ----------
    public void Approve(DateTime utcNow)
    {
        if (Status == VendorStatus.Approved) return;
        Status = VendorStatus.Approved;
        ApprovedAtUtc ??= utcNow;
    }

    public void Block() => Status = VendorStatus.Blocked;

    public void Unblock()
    {
        if (Status != VendorStatus.Blocked) return;
        // Back to Approved only if it had been approved before; otherwise it still needs review
        Status = ApprovedAtUtc is null ? VendorStatus.PendingApproval : VendorStatus.Approved;
    }

    public void Verify() => IsVerified = true;

    // ---------- Ratings ----------
    public void AddRating(int stars)
    {
        if (stars is < 1 or > 5) throw new ArgumentOutOfRangeException(nameof(stars), "Rating must be 1 to 5 stars.");
        Rating = Math.Round((Rating * RatingCount + stars) / (RatingCount + 1), 2);
        RatingCount++;
    }

    // ---------- Balance ----------
    /// <summary>Money from a delivered order is added to the shop's balance.</summary>
    public void Credit(decimal amount, Currency currency)
    {
        if (currency != Currency) throw new InvalidOperationException($"This shop is paid in {Currency}, not {currency}.");
        if (amount <= 0) throw new ArgumentOutOfRangeException(nameof(amount), "Amount must be more than zero.");
        Balance += amount;
    }

    public void Withdraw(decimal amount)
    {
        if (amount <= 0) throw new ArgumentOutOfRangeException(nameof(amount), "Amount must be more than zero.");
        if (amount > Balance) throw new InvalidOperationException("You can't withdraw more than your balance.");
        Balance -= amount;
    }
}