using FluentValidation;
using Phloem.Application.Features.Orders.Commands;
using Phloem.Domain.Entities;

namespace Phloem.Application.Features.Orders.Validators;

/// <summary>Input checks for <see cref="DeclineOrderCommand"/>.</summary>
public sealed class DeclineOrderCommandValidator : AbstractValidator<DeclineOrderCommand>
{
    public DeclineOrderCommandValidator()
    {
        OrderNumberRules.Apply(RuleFor(x => x.OrderNumber));
        RuleFor(x => x.Reason)
            .NotEmpty().WithMessage("Please tell the customer why (e.g. \"We've run out of tilapia today\").")
            .MaximumLength(Order.DeclineReasonMaxLength);
    }
}
