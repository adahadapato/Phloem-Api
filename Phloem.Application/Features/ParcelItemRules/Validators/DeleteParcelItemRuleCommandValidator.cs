using FluentValidation;
using Phloem.Application.Features.ParcelItemRules.Commands;

namespace Phloem.Application.Features.ParcelItemRules.Validators;

/// <summary>Input checks for <see cref="DeleteParcelItemRuleCommand"/>.</summary>
public sealed class DeleteParcelItemRuleCommandValidator : AbstractValidator<DeleteParcelItemRuleCommand>
{
    /// <summary>An id.</summary>
    public DeleteParcelItemRuleCommandValidator()
    {
        RuleFor(x => x.Id).NotEmpty();
    }
}
