using FluentValidation;
using Phloem.Application.Features.Accounts.Commands;
using Phloem.Domain.Entities;
using Phloem.Domain.ValueObjects;

namespace Phloem.Application.Features.Accounts.Validators;

public sealed class AddAddressCommandValidator : AbstractValidator<AddAddressCommand>
{
    public AddAddressCommandValidator()
    {
        RuleFor(x => x.Label).NotEmpty().MaximumLength(DeliveryAddress.LabelMaxLength);
        RuleFor(x => x.FullAddress).NotEmpty().MaximumLength(DeliveryAddress.FullAddressMaxLength);
        RuleFor(x => x.Country).IsInEnum();
        RuleFor(x => x.RecipientName).MaximumLength(DeliveryAddress.RecipientNameMaxLength);
        RuleFor(x => x.PhoneNumber).Must(p => string.IsNullOrWhiteSpace(p) || PhoneNumber.IsValid(p))
            .WithMessage(PhoneNumber.InvalidMessage);
    }
}