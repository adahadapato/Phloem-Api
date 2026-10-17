using FluentValidation;

namespace Phloem.Application.Features.Orders.Validators;

/// <summary>Input checks for <see cref="Commands.PrepareOrderPaymentCommand"/>.</summary>
public sealed class PrepareOrderPaymentCommandValidator : AbstractValidator<Commands.PrepareOrderPaymentCommand>
{
    /// <summary>Order number required.</summary>
    public PrepareOrderPaymentCommandValidator() => OrderNumberRules.Apply(RuleFor(x => x.OrderNumber));
}
