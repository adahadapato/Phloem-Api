namespace Phloem.Domain.Enums;

public enum VendorStatus
{
    PendingApproval = 1,   // just signed up, hidden from buyers
    Approved = 2,          // visible to buyers
    Blocked = 3            // hidden by an admin
}
