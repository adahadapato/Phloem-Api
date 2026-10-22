using FluentValidation;
using Phloem.Application.Features.Vendors.Commands;

namespace Phloem.Application.Features.Vendors.Validators;

/// <summary>Input checks for <see cref="SetMyOrderCutoffCommand"/>.</summary>
public sealed class SetMyOrderCutoffCommandValidator : AbstractValidator<SetMyOrderCutoffCommand>
{
    /// <summary>Earliest cut-off allowed (earlier than this would close a shop before breakfast).</summary>
    public static readonly TimeOnly Earliest = new(8, 0);

    public SetMyOrderCutoffCommandValidator()
    {
        RuleFor(x => x.Cutoff)
            .GreaterThanOrEqualTo(Earliest)
            .WithMessage("Orders for today can close at 8:00 am at the earliest.");
    }
}
