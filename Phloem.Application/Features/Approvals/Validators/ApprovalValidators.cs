using FluentValidation;
using Phloem.Application.Features.Approvals.Commands;
using Phloem.Application.Features.Approvals.Queries;
using Phloem.Domain.Entities;
using Phloem.Domain.Shared.Results;

namespace Phloem.Application.Features.Approvals.Validators;

/// <summary>Checks <see cref="RejectChangeCommand"/>: a change and a reason (the proposer sees it).</summary>
public sealed class RejectChangeCommandValidator : AbstractValidator<RejectChangeCommand>
{
    /// <summary>The rules.</summary>
    public RejectChangeCommandValidator()
    {
        RuleFor(x => x.Id).NotEmpty();
        RuleFor(x => x.Reason).NotEmpty().WithMessage("Please say why, so the person who proposed it knows.")
                              .MaximumLength(PendingChange.DecisionNoteMaxLength);
    }
}

/// <summary>Checks <see cref="ApproveChangeCommand"/>.</summary>
public sealed class ApproveChangeCommandValidator : AbstractValidator<ApproveChangeCommand>
{
    /// <summary>The rules.</summary>
    public ApproveChangeCommandValidator() => RuleFor(x => x.Id).NotEmpty();
}

/// <summary>Checks <see cref="WithdrawChangeCommand"/>.</summary>
public sealed class WithdrawChangeCommandValidator : AbstractValidator<WithdrawChangeCommand>
{
    /// <summary>The rules.</summary>
    public WithdrawChangeCommandValidator() => RuleFor(x => x.Id).NotEmpty();
}

/// <summary>Checks <see cref="GetApprovalsQuery"/>.</summary>
public sealed class GetApprovalsQueryValidator : AbstractValidator<GetApprovalsQuery>
{
    /// <summary>The rules.</summary>
    public GetApprovalsQueryValidator()
    {
        RuleFor(x => x.View).IsInEnum();
        RuleFor(x => x.Page).GreaterThanOrEqualTo(1);
        RuleFor(x => x.PageSize).InclusiveBetween(1, PageRequest.MaxPageSize);
    }
}
