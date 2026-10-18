using FluentValidation;
using Phloem.Application.Features.Vendors.Commands;
using Phloem.Domain.Entities;

namespace Phloem.Application.Features.Vendors.Validators;

public sealed class RejectShopCommandValidator : AbstractValidator<RejectShopCommand>
{
    public RejectShopCommandValidator()
    {
        RuleFor(x => x.ShopId).NotEmpty();
        RuleFor(x => x.Reason)
            .NotEmpty().WithMessage("Please tell the seller why, so they can fix it.")
            .MaximumLength(Vendor.RejectionReasonMaxLength);
    }
}
