using MediatR;
using Phloem.Application.Features.Accounts.Dtos;
using Phloem.Domain.Constants;
using Phloem.Domain.Interfaces;
using Phloem.Domain.Shared.Results;

namespace Phloem.Application.Features.Accounts.Queries;

/// <summary>Handles <see cref="GetMyTwoFactorStatusQuery"/>.</summary>
public sealed class GetMyTwoFactorStatusQueryHandler(ICurrentUser currentUser, IAccountService accounts)
    : IRequestHandler<GetMyTwoFactorStatusQuery, Result<TwoFactorStatusDto>>
{
    /// <summary>Reads the status from Identity and adds whether it's required (admins).</summary>
    /// <exception cref="InvalidOperationException">Called without a signed-in user (endpoint misconfigured).</exception>
    public async Task<Result<TwoFactorStatusDto>> Handle(GetMyTwoFactorStatusQuery query, CancellationToken ct)
    {
        var userId = currentUser.UserId
            ?? throw new InvalidOperationException("This action requires a signed-in user.");

        var status = await accounts.GetTwoFactorStatusAsync(userId, ct);
        if (status.IsFailure) return status.Error;

        return new TwoFactorStatusDto(
            status.Value.IsEnabled,
            status.Value.RecoveryCodesLeft,
            IsRequired: currentUser.IsInRole(Roles.Admin));
    }
}