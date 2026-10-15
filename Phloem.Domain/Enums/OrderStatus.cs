namespace Phloem.Domain.Enums;

public enum OrderStatus
{
    Placed = 1,          // can still be cancelled (within 30 minutes)
    Preparing = 2,
    OutForDelivery = 3,
    Delivered = 4,
    Cancelled = 5
}
