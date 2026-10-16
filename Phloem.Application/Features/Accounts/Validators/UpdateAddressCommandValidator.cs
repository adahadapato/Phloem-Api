using FluentValidation;
using Phloem.Application.Features.Accounts.Commands;
using Phloem.Domain.Entities;

namespace Phloem.Application.Features.Accounts.Validators;

public sealed class UpdateAddressCommandValidator : AbstractValidator<UpdateAddressCommand>
{
    public UpdateAddressCommandValidator()
    {
        RuleFor(x => x.AddressId).NotEmpty();
        RuleFor(x => x.Label).NotEmpty().MaximumLength(DeliveryAddress.LabelMaxLength);
        RuleFor(x => x.FullAddress).NotEmpty().MaximumLength(DeliveryAddress.FullAddressMaxLength);
        RuleFor(x => x.Country).IsInEnum();
    }
}