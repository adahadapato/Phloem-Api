using MediatR;
using Phloem.Domain.Interfaces;
using Phloem.Domain.Shared.Results;

namespace Phloem.Application.Features.Auth.Commands;

public sealed class ResetPasswordCommandHandler(IAuthService auth)
    : IRequestHandler<ResetPasswordCommand, Result>
{
    public Task<Result> Handle(ResetPasswordCommand command, CancellationToken ct) =>
        auth.ResetPasswordAsync(command.Email.Trim(), command.Token, command.NewPassword, ct);
}