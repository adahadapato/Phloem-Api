using FluentValidation;

namespace Phloem.Application.Features.Orders.Validators;

/// <summary>Input checks for <see cref="Queries.GetOrderQuery"/>.</summary>
public sealed class GetOrderQueryValidator : AbstractValidator<Queries.GetOrderQuery>
{
    /// <summary>Order number required.</summary>
    public GetOrderQueryValidator() => OrderNumberRules.Apply(RuleFor(x => x.OrderNumber));
}
