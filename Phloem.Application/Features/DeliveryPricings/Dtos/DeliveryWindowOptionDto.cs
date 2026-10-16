using Phloem.Domain.Enums;

namespace Phloem.Application.Features.DeliveryPricings.Dtos;

/// <summary>
/// One choice in the app's "Delivery time" picker for a country, e.g. "ASAP (30–45 min) · +GHS 8.00".
/// The app writes the label; the server says which windows exist and what each costs extra.
/// </summary>
/// <param name="Window">The delivery window.</param>
/// <param name="ExtraCharge">Extra on top of each shop's delivery fee (0 = no extra).</param>
/// <param name="Currency">Currency of the extra charge.</param>
public sealed record DeliveryWindowOptionDto(DeliveryWindow Window, decimal ExtraCharge, Currency Currency);