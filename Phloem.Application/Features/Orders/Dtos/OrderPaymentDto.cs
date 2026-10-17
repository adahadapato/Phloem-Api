using Phloem.Domain.Enums;

namespace Phloem.Application.Features.Orders.Dtos;

/// <summary>
/// "Pay" tapped: the final amount to charge, after refreshing prices and dropping out-of-stock dishes.
/// </summary>
/// <param name="OrderNumber">The order.</param>
/// <param name="AmountToPay">What the payment provider should charge.</param>
/// <param name="Currency">Currency of the amount.</param>
/// <param name="RemovedDishes">Names of dishes removed because they were out of stock (empty if none).</param>
public sealed record OrderPaymentDto(
    string OrderNumber,
    decimal AmountToPay,
    Currency Currency,
    IReadOnlyList<string> RemovedDishes);