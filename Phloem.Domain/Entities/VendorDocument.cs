using Phloem.Domain.Enums;
using Phloem.Domain.ValueObjects;

namespace Phloem.Domain.Entities;

/// <summary>
/// A document a seller uploaded for an admin to check (registration, hygiene certificate, ID).
/// Belongs to a Vendor and is only created / changed through it. One per type.
/// </summary>
public class VendorDocument : Entity<Guid>
{
    protected VendorDocument() { }

    internal VendorDocument(Guid vendorId, VendorDocumentType type, DocumentFile file, DateTime? expiresAt)
        : base(Guid.CreateVersion7())
    {
        VendorId = vendorId;
        Type = type;
        File = file;
        ExpiresAt = expiresAt;
        Status = DocumentStatus.Pending;
    }

    public Guid VendorId { get; private set; }
    public VendorDocumentType Type { get; private set; }
    public DocumentFile File { get; private set; } = null!;

    public DocumentStatus Status { get; private set; }

    /// <summary>Shown to the seller when rejected, e.g. "The photo is blurry, please upload a clearer copy".</summary>
    public string? RejectionReason { get; private set; }

    /// <summary>Who reviewed it (an admin's Id) and when.</summary>
    public Guid? ReviewedByUserId { get; private set; }
    public DateTime? ReviewedAt { get; private set; }

    /// <summary>Certificates often expire; null if this one doesn't.</summary>
    public DateTime? ExpiresAt { get; private set; }

    public bool IsExpired(DateTime utcNow) => ExpiresAt is not null && ExpiresAt <= utcNow;

    /// <summary>Counts towards verification: accepted and not expired.</summary>
    public bool IsValid(DateTime utcNow) => Status == DocumentStatus.Accepted && !IsExpired(utcNow);

    internal void Accept(Guid adminUserId, DateTime utcNow)
    {
        Status = DocumentStatus.Accepted;
        RejectionReason = null;
        ReviewedByUserId = adminUserId;
        ReviewedAt = utcNow;
    }

    internal void Reject(Guid adminUserId, string reason, DateTime utcNow)
    {
        Status = DocumentStatus.Rejected;
        RejectionReason = reason;
        ReviewedByUserId = adminUserId;
        ReviewedAt = utcNow;
    }
}