using FluentValidation;
using Phloem.Application.Features.Auth.Commands;

namespace Phloem.Application.Features.Auth.Validators;

public sealed class CompleteTwoFactorLoginCommandValidator : AbstractValidator<CompleteTwoFactorLoginCommand>
{
    public CompleteTwoFactorLoginCommandValidator()
    {
        RuleFor(x => x.ChallengeToken).NotEmpty();
        RuleFor(x => x.Code).NotEmpty();
    }
}