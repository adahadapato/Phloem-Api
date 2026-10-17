using FluentValidation;

namespace Phloem.Application.Features.Orders.Validators;

/// <summary>Input checks for <see cref="Queries.GetBuyAgainQuery"/>.</summary>
public sealed class GetBuyAgainQueryValidator : AbstractValidator<Queries.GetBuyAgainQuery>
{
    /// <summary>Order number required.</summary>
    public GetBuyAgainQueryValidator() => OrderNumberRules.Apply(RuleFor(x => x.OrderNumber));
}
