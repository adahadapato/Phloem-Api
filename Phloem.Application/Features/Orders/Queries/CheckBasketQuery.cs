using MediatR;
using Phloem.Application.Features.Orders.Dtos;
using Phloem.Domain.Enums;
using Phloem.Domain.Shared.Results;

namespace Phloem.Application.Features.Orders.Queries;

/// <summary>
/// Checkout page (opening it): checks and prices the app's basket, split by shop.
/// Nothing is saved. Works for guests too (no sign-in needed). Never cached.
/// </summary>
/// <param name="Items">The app's basket; each dish once.</param>
/// <param name="DeliveryWindow">The delivery time the customer picked.</param>
public sealed record CheckBasketQuery(IReadOnlyList<BasketItemDto> Items, DeliveryWindow DeliveryWindow)
    : IRequest<Result<BasketCheckDto>>;