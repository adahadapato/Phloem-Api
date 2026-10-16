using FluentValidation;
using Phloem.Application.Features.Auth.Commands;

namespace Phloem.Application.Features.Auth.Validators;

public sealed class LoginCommandValidator : AbstractValidator<LoginCommand>
{
    public LoginCommandValidator()
    {
        // Only "is anything typed": a format error would reveal more than "wrong email or password"
        RuleFor(x => x.Email).NotEmpty();
        RuleFor(x => x.Password).NotEmpty();
    }
}