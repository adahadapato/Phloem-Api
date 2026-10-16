using Phloem.Domain.Entities;
using Phloem.Domain.Enums;

namespace Phloem.Application.Features.DeliveryPricings.Dtos;

/// <summary>One offered delivery window and its extra charge, e.g. ASAP: +8.00.</summary>
/// <param name="Window">The delivery window.</param>
/// <param name="ExtraCharge">Amount added to the shop's delivery fee for this window.</param>
public sealed record DeliveryWindowChargeDto(DeliveryWindow Window, decimal ExtraCharge)
{
    /// <summary>Maps a window charge.</summary>
    /// <param name="charge">The charge to map.</param>
    public static DeliveryWindowChargeDto From(DeliveryWindowCharge charge) =>
        new(charge.Window, charge.ExtraCharge);
}