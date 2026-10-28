using FluentValidation;
using Phloem.Application.Features.Vendors.Commands;
using Phloem.Domain.Entities;
using Phloem.Domain.ValueObjects;

namespace Phloem.Application.Features.Vendors.Validators;

public sealed class OpenShopCommandValidator : AbstractValidator<OpenShopCommand>
{
    public OpenShopCommandValidator()
    {
        RuleFor(x => x.ShopName).NotEmpty().MaximumLength(Vendor.NameMaxLength);

        // Optional: blank = use the login account's email / phone
        RuleFor(x => x.BusinessEmail)
            .Must(v => string.IsNullOrWhiteSpace(v) || EmailAddress.IsValid(v))
            .WithMessage(EmailAddress.InvalidMessage);

        RuleFor(x => x.BusinessPhone)
            .Must(v => string.IsNullOrWhiteSpace(v) || PhoneNumber.IsValid(v))
            .WithMessage(PhoneNumber.InvalidMessage);

        RuleFor(x => x.AgreedToSellerTerms).Equal(true)
            .WithMessage("Please read and agree to the Seller terms to open your shop.");
    }
}