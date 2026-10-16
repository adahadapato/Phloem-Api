using MediatR;
using Phloem.Domain.Interfaces;
using Phloem.Domain.Shared.Results;

namespace Phloem.Application.Features.Auth.Commands;

public sealed class RevokeTokenCommandHandler(IAuthService auth)
    : IRequestHandler<RevokeTokenCommand, Result>
{
    public Task<Result> Handle(RevokeTokenCommand command, CancellationToken ct) =>
        auth.RevokeTokenAsync(command.RefreshToken, ct);
}