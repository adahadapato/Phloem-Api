using FluentValidation;
using Phloem.Application.Features.Staff.Queries;
using Phloem.Domain.Shared.Results;

namespace Phloem.Application.Features.Staff.Validators;

/// <summary>Checks <see cref="GetStaffAuditQuery"/>: known filters, a sensible date range and page.</summary>
public sealed class GetStaffAuditQueryValidator : AbstractValidator<GetStaffAuditQuery>
{
    /// <summary>The rules.</summary>
    public GetStaffAuditQueryValidator()
    {
        RuleFor(x => x.Action).IsInEnum().When(x => x.Action is not null);
        RuleFor(x => x.Country).IsInEnum().When(x => x.Country is not null);
        RuleFor(x => x.To).GreaterThan(x => x.From)
                          .When(x => x.From is not null && x.To is not null)
                          .WithMessage("The end date must be after the start date.");
        RuleFor(x => x.Page).GreaterThanOrEqualTo(1);
        RuleFor(x => x.PageSize).InclusiveBetween(1, PageRequest.MaxPageSize);
    }
}
