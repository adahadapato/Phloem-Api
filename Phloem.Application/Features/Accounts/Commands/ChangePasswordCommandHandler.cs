using MediatR;
using Phloem.Domain.Interfaces;
using Phloem.Domain.Shared.Results;

namespace Phloem.Application.Features.Accounts.Commands;

/// <summary>Identity checks the current password and hashes the new one.</summary>
public sealed class ChangePasswordCommandHandler(ICurrentUser currentUser, IAccountService accounts)
    : IRequestHandler<ChangePasswordCommand, Result>
{
    public Task<Result> Handle(ChangePasswordCommand command, CancellationToken ct)
    {
        var userId = currentUser.UserId
            ?? throw new InvalidOperationException("This action requires a signed-in user.");

        return accounts.ChangePasswordAsync(userId, command.CurrentPassword, command.NewPassword, ct);
    }
}