using FluentValidation;
using Phloem.Application.Features.Vendors.Commands;

namespace Phloem.Application.Features.Vendors.Validators;

public sealed class SetShopVerifiedCommandValidator : AbstractValidator<SetShopVerifiedCommand>
{
    public SetShopVerifiedCommandValidator()
    {
        RuleFor(x => x.ShopId).NotEmpty();
    }
}
