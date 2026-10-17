using MediatR;
using Phloem.Domain.Interfaces;
using Phloem.Domain.Shared.Results;

namespace Phloem.Application.Features.Accounts.Commands;

/// <summary>Handles <see cref="EnableTwoFactorCommand"/>.</summary>
public sealed class EnableTwoFactorCommandHandler(ICurrentUser currentUser, IAccountService accounts)
    : IRequestHandler<EnableTwoFactorCommand, Result<IReadOnlyCollection<string>>>
{
    /// <summary>Identity checks the code; a wrong code returns AuthErrors.InvalidTwoFactorCode.</summary>
    /// <exception cref="InvalidOperationException">Called without a signed-in user (endpoint misconfigured).</exception>
    public Task<Result<IReadOnlyCollection<string>>> Handle(EnableTwoFactorCommand command, CancellationToken ct)
    {
        var userId = currentUser.UserId
            ?? throw new InvalidOperationException("This action requires a signed-in user.");

        return accounts.EnableTwoFactorAsync(userId, command.Code.Replace(" ", string.Empty), ct);
    }
}