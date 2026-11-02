using FluentValidation;

namespace Phloem.Application.Features.Orders.Validators;

/// <summary>Input checks for <see cref="Queries.GetAdminOrderQuery"/>.</summary>
public sealed class GetAdminOrderQueryValidator : AbstractValidator<Queries.GetAdminOrderQuery>
{
    /// <summary>Order number required.</summary>
    public GetAdminOrderQueryValidator() => OrderNumberRules.Apply(RuleFor(x => x.OrderNumber));
}
