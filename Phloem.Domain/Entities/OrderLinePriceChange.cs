using Phloem.Domain.Enums;

namespace Phloem.Domain.Entities;

/// <summary>
/// One price change in an unpaid order that the customer hasn't been told about yet.
/// Produced by <see cref="Order.TakeUnnotifiedPriceChanges"/> for the weekly
/// "prices changed in your saved orders" email.
/// </summary>
/// <param name="OrderNumber">The order the dish is in, e.g. "PH-240518-4821".</param>
/// <param name="DishName">The dish's name, as shown in the order.</param>
/// <param name="OldPrice">The price the customer was last told about.</param>
/// <param name="NewPrice">Today's price.</param>
/// <param name="Currency">The currency of both prices.</param>
public sealed record OrderLinePriceChange(
    string OrderNumber,
    string DishName,
    decimal OldPrice,
    decimal NewPrice,
    Currency Currency);