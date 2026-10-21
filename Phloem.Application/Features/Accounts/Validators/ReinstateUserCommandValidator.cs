using FluentValidation;
using Phloem.Application.Features.Accounts.Commands;

namespace Phloem.Application.Features.Accounts.Validators;

/// <summary>Input checks for <see cref="ReinstateUserCommand"/>.</summary>
public sealed class ReinstateUserCommandValidator : AbstractValidator<ReinstateUserCommand>
{
    public ReinstateUserCommandValidator() => RuleFor(x => x.UserId).NotEmpty();
}
