using FluentValidation;
using Phloem.Application.Features.Orders.Commands;

namespace Phloem.Application.Features.Orders.Validators;

/// <summary>Input checks for <see cref="MarkOrderDeliveredCommand"/>.</summary>
public sealed class MarkOrderDeliveredCommandValidator : AbstractValidator<MarkOrderDeliveredCommand>
{
    /// <summary>Order number required.</summary>
    public MarkOrderDeliveredCommandValidator() => OrderNumberRules.Apply(RuleFor(x => x.OrderNumber));
}
