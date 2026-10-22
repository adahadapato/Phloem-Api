using Phloem.Domain.Enums;

namespace Phloem.Application.Features.Orders.Dtos;

/// <summary>
/// One shop's part of the basket. Each shop becomes its own order, with its own delivery fee.
/// </summary>
/// <param name="VendorId">The shop (handle only, never shown).</param>
/// <param name="ShopName">The shop's name.</param>
/// <param name="Currency">The shop's currency.</param>
/// <param name="Lines">This shop's dishes.</param>
/// <param name="Subtotal">Sum of the AVAILABLE lines.</param>
/// <param name="DeliveryFee">Shop fee + delivery-window extra; null if delivery can't be priced.</param>
/// <param name="DeliveryProblem">Why there's no fee (e.g. "ASAP delivery isn't offered here"); null if fine.</param>
/// <param name="Total">Subtotal + DeliveryFee; null if the fee couldn't be worked out.</param>
/// <param name="OpenWindows">The delivery times this shop takes RIGHT NOW (after its cut-off: only Tomorrow), among
/// those offered in its country. The app's "Delivery time" picker shows only these.</param>
/// <param name="OrderCutoffTime">When the shop stops taking orders for today (its local time), e.g. 19:00.</param>
public sealed record BasketShopDto(
    Guid VendorId,
    string ShopName,
    Currency Currency,
    IReadOnlyList<BasketLineDto> Lines,
    decimal Subtotal,
    decimal? DeliveryFee,
    string? DeliveryProblem,
    decimal? Total,
    IReadOnlyList<DeliveryWindow> OpenWindows,
    TimeOnly OrderCutoffTime);