using FluentValidation;
using Phloem.Application.Features.Vendors.Commands;
using Phloem.Domain.Entities;
using Phloem.Domain.ValueObjects;

namespace Phloem.Application.Features.Vendors.Validators;

/// <summary>Same rules as opening a shop.</summary>
public sealed class UpdateShopDetailsCommandValidator : AbstractValidator<UpdateShopDetailsCommand>
{
    public UpdateShopDetailsCommandValidator()
    {
        RuleFor(x => x.ShopName).NotEmpty().MaximumLength(Vendor.NameMaxLength);

        RuleFor(x => x.BusinessEmail)
            .Must(v => string.IsNullOrWhiteSpace(v) || EmailAddress.IsValid(v))
            .WithMessage(EmailAddress.InvalidMessage);

        RuleFor(x => x.BusinessPhone)
            .Must(v => string.IsNullOrWhiteSpace(v) || PhoneNumber.IsValid(v))
            .WithMessage(PhoneNumber.InvalidMessage);
    }
}