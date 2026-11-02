using FluentValidation;
using Phloem.Application.Features.Staff.Commands;
using Phloem.Domain.Constants;

namespace Phloem.Application.Features.Staff.Validators;

/// <summary>Checks <see cref="SetJobRolePermissionsCommand"/>: a job role and only known permissions.</summary>
public sealed class SetJobRolePermissionsCommandValidator : AbstractValidator<SetJobRolePermissionsCommand>
{
    /// <summary>The rules.</summary>
    public SetJobRolePermissionsCommandValidator()
    {
        RuleFor(x => x.Role)
            .NotEmpty()
            .Must(Roles.IsJobRole).WithMessage("That isn't a staff job role.");

        RuleFor(x => x.Permissions).NotNull();

        RuleForEach(x => x.Permissions)
            .Must(Permissions.IsKnown).WithMessage("'{PropertyValue}' isn't a permission Phloem knows.");
    }
}
