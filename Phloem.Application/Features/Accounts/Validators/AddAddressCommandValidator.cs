using FluentValidation;
using Phloem.Application.Features.Accounts.Commands;
using Phloem.Domain.Entities;

namespace Phloem.Application.Features.Accounts.Validators;

public sealed class AddAddressCommandValidator : AbstractValidator<AddAddressCommand>
{
    public AddAddressCommandValidator()
    {
        RuleFor(x => x.Label).NotEmpty().MaximumLength(DeliveryAddress.LabelMaxLength);
        RuleFor(x => x.FullAddress).NotEmpty().MaximumLength(DeliveryAddress.FullAddressMaxLength);
        RuleFor(x => x.Country).IsInEnum();
    }
}