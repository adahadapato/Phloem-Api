using FluentValidation;
using Phloem.Application.Features.Vendors.Commands;

namespace Phloem.Application.Features.Vendors.Validators;

public sealed class ApproveShopCommandValidator : AbstractValidator<ApproveShopCommand>
{
    public ApproveShopCommandValidator()
    {
        RuleFor(x => x.ShopId).NotEmpty();
    }
}
