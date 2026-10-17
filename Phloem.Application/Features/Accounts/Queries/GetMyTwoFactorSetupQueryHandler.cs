using System.Text.RegularExpressions;
using MediatR;
using Phloem.Application.Features.Accounts.Dtos;
using Phloem.Domain.Interfaces;
using Phloem.Domain.Shared.Results;

namespace Phloem.Application.Features.Accounts.Queries;

/// <summary>Handles <see cref="GetMyTwoFactorSetupQuery"/>.</summary>
public sealed partial class GetMyTwoFactorSetupQueryHandler(ICurrentUser currentUser, IAccountService accounts)
    : IRequestHandler<GetMyTwoFactorSetupQuery, Result<TwoFactorSetupDto>>
{
    /// <summary>Gets the key from Identity and formats it for typing in by hand.</summary>
    /// <exception cref="InvalidOperationException">Called without a signed-in user (endpoint misconfigured).</exception>
    public async Task<Result<TwoFactorSetupDto>> Handle(GetMyTwoFactorSetupQuery query, CancellationToken ct)
    {
        var userId = currentUser.UserId
            ?? throw new InvalidOperationException("This action requires a signed-in user.");

        var setup = await accounts.GetTwoFactorSetupAsync(userId, ct);
        if (setup.IsFailure) return setup.Error;

        return new TwoFactorSetupDto(GroupsOfFour(setup.Value.SharedKey), setup.Value.AuthenticatorUri);
    }

    /// <summary>"abcdefgh" → "ABCD EFGH": easier to read and type.</summary>
    private static string GroupsOfFour(string key) =>
        EveryFourChars().Replace(key.ToUpperInvariant(), "$0 ").Trim();

    [GeneratedRegex(".{4}")]
    private static partial Regex EveryFourChars();
}