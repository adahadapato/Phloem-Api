using FluentValidation;
using Phloem.Application.Features.Orders.Commands;

namespace Phloem.Application.Features.Orders.Validators;

/// <summary>Input checks for <see cref="AcceptOrderCommand"/>.</summary>
public sealed class AcceptOrderCommandValidator : AbstractValidator<AcceptOrderCommand>
{
    /// <summary>Order number required.</summary>
    public AcceptOrderCommandValidator() => OrderNumberRules.Apply(RuleFor(x => x.OrderNumber));
}
