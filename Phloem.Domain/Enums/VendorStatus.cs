namespace Phloem.Domain.Enums;

public enum VendorStatus
{
    PendingApproval = 1,   // just signed up (or fixed after a rejection), hidden from buyers
    Approved = 2,          // visible to buyers
    Blocked = 3,           // hidden by an admin
    Rejected = 4,          // admin said no, with a reason; seller can fix details → back to PendingApproval
    Closed = 5             // owner deleted their account (row kept for old orders, soft-deleted)
}