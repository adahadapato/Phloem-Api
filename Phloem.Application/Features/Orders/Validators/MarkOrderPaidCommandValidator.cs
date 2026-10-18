using FluentValidation;
using Phloem.Domain.Entities;

namespace Phloem.Application.Features.Orders.Validators;

/// <summary>Input checks for <see cref="Commands.MarkOrderPaidCommand"/>.</summary>
public sealed class MarkOrderPaidCommandValidator : AbstractValidator<Commands.MarkOrderPaidCommand>
{
    /// <summary>Order number and payment reference required.</summary>
    public MarkOrderPaidCommandValidator()
    {
        OrderNumberRules.Apply(RuleFor(x => x.OrderNumber));
        RuleFor(x => x.PaymentReference).NotEmpty().MaximumLength(Order.PaymentReferenceMaxLength);
    }
}
