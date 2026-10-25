using FluentValidation;
using Phloem.Application.Features.Accounts.Commands;

namespace Phloem.Application.Features.Accounts.Validators;

/// <summary>Input checks for <see cref="SetUserRoleCommand"/>.</summary>
public sealed class SetUserRoleCommandValidator : AbstractValidator<SetUserRoleCommand>
{
    /// <summary>A person and a role name (whether the role can be changed is checked by the handler).</summary>
    public SetUserRoleCommandValidator()
    {
        RuleFor(x => x.UserId).NotEmpty();
        RuleFor(x => x.Role).NotEmpty().MaximumLength(50);
    }
}
