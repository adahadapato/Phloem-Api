using Phloem.Domain.Enums;

namespace Phloem.Application.Features.DeliveryPricings.Dtos;

/// <summary>
/// Shop settings → Delivery fee: the seller's fee and the range they may choose from.
/// </summary>
/// <param name="CurrentFee">The fee customers pay this shop (before any delivery-window extra).</param>
/// <param name="UsesCountryMinimum">True if the seller hasn't set a fee, so the country minimum is used.</param>
/// <param name="MinAllowed">Lowest fee allowed (inclusive).</param>
/// <param name="MaxAllowed">Highest fee allowed (inclusive).</param>
/// <param name="Currency">Currency of all amounts.</param>
public sealed record MyDeliveryFeeDto(
    decimal CurrentFee,
    bool UsesCountryMinimum,
    decimal MinAllowed,
    decimal MaxAllowed,
    Currency Currency);