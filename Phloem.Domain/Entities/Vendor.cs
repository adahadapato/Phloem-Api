using Phloem.Domain.Enums;
using Phloem.Domain.Errors;
using Phloem.Domain.Shared;
using Phloem.Domain.Shared.Results;
using Phloem.Domain.ValueObjects;

namespace Phloem.Domain.Entities;

/// <summary>
/// A seller's shop. A user becomes a seller by owning one.
/// New shops wait for admin approval before buyers can see them.
///
/// Errors: business-rule failures are RETURNED as a Result (see VendorErrors);
/// programmer mistakes (nulls, blank names, amounts ≤ 0, wrong currency) still THROW.
/// </summary>
public class Vendor : Entity<Guid>
{
    /// <summary>
    /// Documents that must be Accepted (and not expired) before an admin can give the ✓ badge.
    /// A business decision: change this list if the requirements change.
    /// </summary>
    public static readonly IReadOnlyList<VendorDocumentType> RequiredDocuments =
        [VendorDocumentType.BusinessRegistration, VendorDocumentType.FoodHygieneCertificate];

    private readonly List<VendorDocument> _documents = new();

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

    // ---------- Shop images (public container) ----------
    public ImageFile? Logo { get; private set; }                   // round picture on shop cards
    public ImageFile? CoverPhoto { get; private set; }             // banner on the shop page

    // ---------- Status ----------
    public VendorStatus Status { get; private set; }
    public bool IsVerified { get; private set; }                    // the ✓ badge
    public bool IsVisibleToBuyers => Status == VendorStatus.Approved;

    // ---------- Reputation & money ----------
    public decimal Rating { get; private set; }                     // average stars, 0–5
    public int RatingCount { get; private set; }
    public decimal Balance { get; private set; }                    // in this shop's Currency

    // ---------- Documents (private container) ----------
    public virtual IReadOnlyCollection<VendorDocument> Documents => _documents.AsReadOnly();

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
    public static Result<Vendor> Open(UserProfile owner, string name, EmailAddress businessEmail, PhoneNumber? businessPhone)
    {
        ArgumentNullException.ThrowIfNull(owner);
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        ArgumentNullException.ThrowIfNull(businessEmail);

        if (owner.IsDeleted) return VendorErrors.AccountDeleted;

        return new Vendor(Guid.CreateVersion7(), owner.Id, name.Trim(),
            businessEmail, businessPhone, owner.Country);
    }

    /// <summary>Shop settings page. Blank email / phone are filled in by the Application layer (as in Open).</summary>
    public Result UpdateDetails(string name, EmailAddress businessEmail, PhoneNumber? businessPhone)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        ArgumentNullException.ThrowIfNull(businessEmail);

        if (Status == VendorStatus.Blocked) return VendorErrors.Blocked;

        Name = name.Trim();
        BusinessEmail = businessEmail;
        BusinessPhone = businessPhone;
        return Result.Success();
    }

    // ---------- Admin actions ----------
    /// <summary>Admin approves a new shop. A blocked shop must be unblocked first.</summary>
    public Result Approve(DateTime utcNow)
    {
        if (Status == VendorStatus.Approved) return Result.Success();   // approving twice is fine
        if (Status == VendorStatus.Blocked) return VendorErrors.Blocked;

        Status = VendorStatus.Approved;
        ApprovedAtUtc ??= utcNow;
        return Result.Success();
    }

    public void Block() => Status = VendorStatus.Blocked;

    public void Unblock()
    {
        if (Status != VendorStatus.Blocked) return;
        // Back to Approved only if it had been approved before; otherwise it still needs review
        Status = ApprovedAtUtc is null ? VendorStatus.PendingApproval : VendorStatus.Approved;
    }

    // ---------- Verification (✓ badge) ----------

    /// <summary>Required documents that are missing, rejected, pending or expired.</summary>
    public IReadOnlyList<VendorDocumentType> MissingDocuments(DateTime utcNow) =>
        RequiredDocuments
            .Where(type => !_documents.Any(d => d.Type == type && d.IsValid(utcNow)))
            .ToList();

    /// <summary>Admin gives the ✓ badge. Only possible once every required document is accepted.</summary>
    public Result Verify(DateTime utcNow)
    {
        if (Status == VendorStatus.Blocked) return VendorErrors.Blocked;

        var missing = MissingDocuments(utcNow);
        if (missing.Count > 0) return VendorErrors.MissingDocuments(missing);

        IsVerified = true;
        return Result.Success();
    }

    /// <summary>
    /// Removes the ✓ badge. Also called automatically when a required document is replaced or rejected,
    /// and by a daily job when a required document expires.
    /// </summary>
    public void Unverify() => IsVerified = false;

    // ---------- Documents ----------

    /// <summary>
    /// Seller uploads a document. One per type: a new upload replaces the old one (e.g. a renewed
    /// certificate). The result includes the replaced file, if any, so the Application layer can
    /// delete it from storage after saving.
    /// </summary>
    public Result<DocumentSubmitted> SubmitDocument(VendorDocumentType type, DocumentFile file,
                                                    DateTime? expiresAt, DateTime utcNow)
    {
        ArgumentNullException.ThrowIfNull(file);

        if (Status == VendorStatus.Blocked) return VendorErrors.Blocked;
        if (expiresAt is not null && expiresAt <= utcNow) return VendorErrors.DocumentExpired;

        var old = _documents.FirstOrDefault(d => d.Type == type);
        if (old is not null)
            _documents.Remove(old);

        var document = new VendorDocument(Id, type, file, expiresAt);
        _documents.Add(document);

        // A required document is being re-checked, so the badge waits for the new review
        if (RequiredDocuments.Contains(type))
            Unverify();

        return new DocumentSubmitted(document, old?.File);
    }

    /// <summary>Admin accepts a document.</summary>
    public Result AcceptDocument(Guid documentId, Guid adminUserId, DateTime utcNow)
    {
        var found = FindPendingDocument(documentId);
        if (found.IsFailure) return found;

        found.Value.Accept(adminUserId, utcNow);
        return Result.Success();
    }

    /// <summary>Admin rejects a document, with a reason the seller will see.</summary>
    public Result RejectDocument(Guid documentId, Guid adminUserId, string reason, DateTime utcNow)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(reason);   // the validator asks for a reason first

        var found = FindPendingDocument(documentId);
        if (found.IsFailure) return found;

        found.Value.Reject(adminUserId, reason.Trim(), utcNow);

        if (RequiredDocuments.Contains(found.Value.Type))
            Unverify();

        return Result.Success();
    }

    private Result<VendorDocument> FindPendingDocument(Guid documentId)
    {
        var document = _documents.FirstOrDefault(d => d.Id == documentId);
        if (document is null) return VendorErrors.DocumentNotFound;
        if (document.Status != DocumentStatus.Pending) return VendorErrors.DocumentAlreadyReviewed;
        return document;
    }

    // ---------- Shop images ----------

    /// <summary>Returns the old logo's file (if any) so the Application layer can delete it from storage.</summary>
    public ImageFile? ChangeLogo(ImageFile logo)
    {
        ArgumentNullException.ThrowIfNull(logo);
        var old = Logo;
        Logo = logo;
        return old;
    }

    public ImageFile? RemoveLogo()
    {
        var old = Logo;
        Logo = null;
        return old;
    }

    /// <summary>Returns the old cover's file (if any) so the Application layer can delete it from storage.</summary>
    public ImageFile? ChangeCoverPhoto(ImageFile cover)
    {
        ArgumentNullException.ThrowIfNull(cover);
        var old = CoverPhoto;
        CoverPhoto = cover;
        return old;
    }

    public ImageFile? RemoveCoverPhoto()
    {
        var old = CoverPhoto;
        CoverPhoto = null;
        return old;
    }

    // ---------- Ratings ----------
    /// <summary>Stars are checked by the validator first, so out-of-range is a bug.</summary>
    public void AddRating(int stars)
    {
        ArgumentOutOfRangeException.ThrowIfLessThan(stars, 1);
        ArgumentOutOfRangeException.ThrowIfGreaterThan(stars, 5);

        Rating = Math.Round((Rating * RatingCount + stars) / (RatingCount + 1), 2);
        RatingCount++;
    }

    // ---------- Balance ----------
    /// <summary>
    /// Money from a delivered order is added to the shop's balance.
    /// Called by the system, never typed by a user, so a wrong currency or amount is a bug.
    /// </summary>
    public void Credit(decimal amount, Currency currency)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(amount);
        if (currency != Currency)
            throw new InvalidOperationException($"This shop is paid in {Currency}, not {currency}.");

        Balance += amount;
    }

    /// <summary>The seller asks to be paid out.</summary>
    public Result Withdraw(decimal amount)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(amount);   // the validator catches this first

        if (Status == VendorStatus.Blocked) return VendorErrors.Blocked;
        if (amount > Balance) return VendorErrors.InsufficientBalance;

        Balance -= amount;
        return Result.Success();
    }
}