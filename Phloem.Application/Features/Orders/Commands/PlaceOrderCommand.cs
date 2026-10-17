using MediatR;
using Phloem.Application.Features.Orders.Dtos;
using Phloem.Domain.Enums;
using Phloem.Domain.Shared.Results;

namespace Phloem.Application.Features.Orders.Commands;

/// <summary>
/// Checkout → "Place order". Creates one UNPAID order per shop in the basket.
/// Everything is re-checked on the server; prices and fees are never taken from the app.
/// All orders are saved together, or none are.
/// </summary>
/// <param name="Items">The app's basket; each dish once.</param>
/// <param name="DeliveryAddressId">One of the customer's saved addresses.</param>
/// <param name="DeliveryWindow">ASAP / this evening / tomorrow.</param>
public sealed record PlaceOrderCommand(
    IReadOnlyList<BasketItemDto> Items,
    Guid DeliveryAddressId,
    DeliveryWindow DeliveryWindow) : IRequest<Result<IReadOnlyList<PlacedOrderDto>>>;