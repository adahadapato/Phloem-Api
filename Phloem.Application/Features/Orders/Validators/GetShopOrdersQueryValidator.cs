using FluentValidation;
using Phloem.Application.Features.Orders.Queries;
using Phloem.Domain.Enums;

namespace Phloem.Application.Features.Orders.Validators;

/// <summary>Input checks for <see cref="GetShopOrdersQuery"/>.</summary>
public sealed class GetShopOrdersQueryValidator : AbstractValidator<GetShopOrdersQuery>
{
    /// <summary>Status must be real, and sellers can never ask for unpaid orders.</summary>
    public GetShopOrdersQueryValidator()
    {
        RuleFor(x => x.Status)
            .IsInEnum()
            .NotEqual(OrderStatus.AwaitingPayment).WithMessage("Unpaid orders aren't shown to shops.")
            .When(x => x.Status is not null);
    }
}
