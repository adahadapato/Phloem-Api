using FluentValidation;

namespace Phloem.Application.Features.Orders.Validators;

/// <summary>Input checks for <see cref="Commands.CancelOrderCommand"/>.</summary>
public sealed class CancelOrderCommandValidator : AbstractValidator<Commands.CancelOrderCommand>
{
    /// <summary>Order number required.</summary>
    public CancelOrderCommandValidator() => OrderNumberRules.Apply(RuleFor(x => x.OrderNumber));
}