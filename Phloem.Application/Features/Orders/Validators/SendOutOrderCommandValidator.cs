using FluentValidation;
using Phloem.Application.Features.Orders.Commands;

namespace Phloem.Application.Features.Orders.Validators;

/// <summary>Input checks for <see cref="SendOutOrderCommand"/>.</summary>
public sealed class SendOutOrderCommandValidator : AbstractValidator<SendOutOrderCommand>
{
    /// <summary>Order number required; a courier Id, when given, isn't empty.</summary>
    public SendOutOrderCommandValidator()
    {
        OrderNumberRules.Apply(RuleFor(x => x.OrderNumber));
        RuleFor(x => x.CourierId).NotEqual(Guid.Empty).WithMessage("Choose a courier.");
    }
}
