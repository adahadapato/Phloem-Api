using FluentValidation;
using Phloem.Application.Features.Orders.Queries;

namespace Phloem.Application.Features.Orders.Validators;

/// <summary>Input checks for <see cref="CheckBasketQuery"/>.</summary>
public sealed class CheckBasketQueryValidator : AbstractValidator<CheckBasketQuery>
{
    /// <summary>Basket rules + a real delivery window.</summary>
    public CheckBasketQueryValidator()
    {
        BasketItemsRules.Apply(RuleFor(x => x.Items));
        RuleFor(x => x.DeliveryWindow).IsInEnum();
    }
}