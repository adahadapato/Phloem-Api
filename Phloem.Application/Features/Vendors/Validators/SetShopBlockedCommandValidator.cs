using FluentValidation;
using Phloem.Application.Features.Vendors.Commands;

namespace Phloem.Application.Features.Vendors.Validators;

public sealed class SetShopBlockedCommandValidator : AbstractValidator<SetShopBlockedCommand>
{
    public SetShopBlockedCommandValidator()
    {
        RuleFor(x => x.ShopId).NotEmpty();
    }
}
