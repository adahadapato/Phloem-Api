using MediatR;
using Phloem.Domain.Interfaces;
using Phloem.Domain.Shared.Results;

namespace Phloem.Application.Features.Auth.Commands;

public sealed class ForgotPasswordCommandHandler(IAuthService auth)
    : IRequestHandler<ForgotPasswordCommand, Result>
{
    public Task<Result> Handle(ForgotPasswordCommand command, CancellationToken ct) =>
        auth.SendPasswordResetAsync(command.Email.Trim(), ct);
}