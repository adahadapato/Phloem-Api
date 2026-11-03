using FluentValidation;
using Phloem.Application.Features.Moderation.Commands;
using Phloem.Application.Features.Moderation.Queries;
using Phloem.Domain.Entities;

namespace Phloem.Application.Features.Moderation.Validators;

/// <summary>Input checks for <see cref="DecideModerationCommand"/>.</summary>
public sealed class DecideModerationCommandValidator : AbstractValidator<DecideModerationCommand>
{
    /// <summary>A reason is required to reject (the seller sees it), and kept short.</summary>
    public DecideModerationCommandValidator()
    {
        RuleFor(x => x.Id).NotEmpty();
        RuleFor(x => x.Reason).NotEmpty().When(x => !x.Approve)
                              .WithMessage("Say why it isn't allowed: the seller will see it.");
        RuleFor(x => x.Reason).MaximumLength(Dish.ReviewNoteMaxLength);
    }
}

/// <summary>Input checks for <see cref="GetModerationQueueQuery"/>.</summary>
public sealed class GetModerationQueueQueryValidator : AbstractValidator<GetModerationQueueQuery>
{
    /// <summary>The kind must be real when given.</summary>
    public GetModerationQueueQueryValidator() => RuleFor(x => x.Kind).IsInEnum();
}
