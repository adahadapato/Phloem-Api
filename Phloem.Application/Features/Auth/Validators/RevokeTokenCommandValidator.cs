using FluentValidation;
using Phloem.Application.Features.Auth.Commands;

namespace Phloem.Application.Features.Auth.Validators;

public sealed class RevokeTokenCommandValidator : AbstractValidator<RevokeTokenCommand>
{
    public RevokeTokenCommandValidator()
    {
        RuleFor(x => x.RefreshToken).NotEmpty();
    }
}