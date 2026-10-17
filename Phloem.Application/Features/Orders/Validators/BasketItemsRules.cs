using FluentValidation;
using Phloem.Application.Features.Orders.Dtos;
using Phloem.Domain.Entities;

namespace Phloem.Application.Features.Orders.Validators;

/// <summary>Basket rules shared by <see cref="CheckBasketQueryValidator"/> and <see cref="PlaceOrderCommandValidator"/>.</summary>
internal static class BasketItemsRules
{
    /// <summary>Most different dishes in one basket (a request-size guard, not a business rule).</summary>
    public const int MaxDishes = 50;

    /// <summary>Applies the rules to a basket property.</summary>
    /// <typeparam name="T">The command or query.</typeparam>
    /// <param name="rule">The rule builder for the Items property.</param>
    public static void Apply<T>(IRuleBuilderInitial<T, IReadOnlyList<BasketItemDto>> rule)
    {
        rule.NotEmpty().WithMessage("Your basket is empty.")
            .Must(items => items.Count <= MaxDishes)
                .WithMessage($"A basket can hold at most {MaxDishes} different dishes.")
            .Must(items => items.Select(i => i.DishId).Distinct().Count() == items.Count)
                .WithMessage("Each dish should appear only once in the basket.")
            .ForEach(item => item.ChildRules(line =>
            {
                line.RuleFor(i => i.DishId).NotEmpty();
                line.RuleFor(i => i.Quantity).InclusiveBetween(1, Order.MaxQuantityPerDish);
            }));
    }
}