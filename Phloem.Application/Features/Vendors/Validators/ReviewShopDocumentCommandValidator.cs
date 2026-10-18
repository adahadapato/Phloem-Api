using FluentValidation;
using Phloem.Application.Features.Vendors.Commands;
using Phloem.Domain.Entities;

namespace Phloem.Application.Features.Vendors.Validators;

public sealed class ReviewShopDocumentCommandValidator : AbstractValidator<ReviewShopDocumentCommand>
{
    public ReviewShopDocumentCommandValidator()
    {
        RuleFor(x => x.ShopId).NotEmpty();
        RuleFor(x => x.DocumentId).NotEmpty();

        RuleFor(x => x.Reason)
            .NotEmpty().WithMessage("Please tell the seller why the document wasn't accepted.")
            .MaximumLength(Vendor.RejectionReasonMaxLength)
            .When(x => !x.Accept);
    }
}
