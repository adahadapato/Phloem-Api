namespace Phloem.Domain.Enums;

public enum DocumentStatus
{
    Pending = 1,    // uploaded, waiting for an admin
    Accepted = 2,
    Rejected = 3    // the seller sees the reason and can upload a new one
}