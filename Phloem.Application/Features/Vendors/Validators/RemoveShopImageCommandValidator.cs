using FluentValidation;
using Phloem.Application.Features.Vendors.Commands;

namespace Phloem.Application.Features.Vendors.Validators;

/// <summary>The picture must be a real one: Logo or CoverPhoto.</summary>
public sealed class RemoveShopImageCommandValidator : AbstractValidator<RemoveShopImageCommand>
{
    public RemoveShopImageCommandValidator()
    {
        RuleFor(x => x.Image).IsInEnum();
    }
}