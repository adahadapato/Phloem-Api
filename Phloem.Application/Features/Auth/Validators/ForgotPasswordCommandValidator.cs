using FluentValidation;
using Phloem.Application.Features.Auth.Commands;
using Phloem.Domain.ValueObjects;

namespace Phloem.Application.Features.Auth.Validators;

public sealed class ForgotPasswordCommandValidator : AbstractValidator<ForgotPasswordCommand>
{
    public ForgotPasswordCommandValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty()
            .Must(EmailAddress.IsValid).WithMessage(EmailAddress.InvalidMessage);
    }
}