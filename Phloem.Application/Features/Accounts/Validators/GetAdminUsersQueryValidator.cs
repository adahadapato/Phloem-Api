using FluentValidation;
using Phloem.Application.Features.Accounts.Queries;
using Phloem.Domain.Shared.Results;

namespace Phloem.Application.Features.Accounts.Validators;

/// <summary>Input checks for <see cref="GetAdminUsersQuery"/>.</summary>
public sealed class GetAdminUsersQueryValidator : AbstractValidator<GetAdminUsersQuery>
{
    /// <summary>Longest search text accepted.</summary>
    public const int SearchMaxLength = 100;

    public GetAdminUsersQueryValidator()
    {
        RuleFor(x => x.Search).MaximumLength(SearchMaxLength);
        RuleFor(x => x.Page).GreaterThanOrEqualTo(1);
        RuleFor(x => x.PageSize).InclusiveBetween(1, PageRequest.MaxPageSize);
    }
}
