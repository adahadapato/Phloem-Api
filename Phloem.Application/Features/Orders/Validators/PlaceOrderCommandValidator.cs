using FluentValidation;
using Phloem.Application.Features.Orders.Commands;

namespace Phloem.Application.Features.Orders.Validators;

/// <summary>Input checks for <see cref="PlaceOrderCommand"/>.</summary>
public sealed class PlaceOrderCommandValidator : AbstractValidator<PlaceOrderCommand>
{
    /// <summary>Basket rules + an address + a real delivery window.</summary>
    public PlaceOrderCommandValidator()
    {
        BasketItemsRules.Apply(RuleFor(x => x.Items));
        RuleFor(x => x.DeliveryAddressId).NotEmpty().WithMessage("Please choose a delivery address.");
        RuleFor(x => x.DeliveryWindow).IsInEnum();
    }
}