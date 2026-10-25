using FluentValidation;
using Phloem.Application.Features.ParcelItemRules.Commands;

namespace Phloem.Application.Features.ParcelItemRules.Validators;

/// <summary>Input checks for <see cref="ReorderParcelItemRulesCommand"/>.</summary>
public sealed class ReorderParcelItemRulesCommandValidator : AbstractValidator<ReorderParcelItemRulesCommand>
{
    /// <summary>At least one id, none empty, none twice.</summary>
    public ReorderParcelItemRulesCommandValidator()
    {
        RuleFor(x => x.Ids).NotEmpty()
            .Must(ids => ids.Distinct().Count() == ids.Count).WithMessage("Each item can only appear once.");
        RuleForEach(x => x.Ids).NotEmpty();
    }
}
