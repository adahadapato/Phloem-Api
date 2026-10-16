using Phloem.Domain.Enums;

namespace Phloem.Domain.Entities;

/// <summary>
/// A shop's delivery fee was moved into the admin's new range. Returned by
/// <see cref="Vendor.ApplyDeliveryFeeRange"/> so the Application layer can email the seller.
/// </summary>
/// <param name="OldFee">The fee before the change.</param>
/// <param name="NewFee">The fee after the change (the nearest limit).</param>
/// <param name="Currency">Currency of both fees.</param>
public sealed record DeliveryFeeAdjustment(decimal OldFee, decimal NewFee, Currency Currency);