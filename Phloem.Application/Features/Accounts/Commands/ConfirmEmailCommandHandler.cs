using MediatR;
using Phloem.Domain.Interfaces;
using Phloem.Domain.Shared.Results;

namespace Phloem.Application.Features.Auth.Commands;

/// <summary>Confirms the email address; an expired or damaged link returns <c>AuthErrors.InvalidToken</c>.</summary>
public sealed class ConfirmEmailCommandHandler(IAuthService auth) : IRequestHandler<ConfirmEmailCommand, Result>
{
    /// <summary>Hands the link's values to Identity.</summary>
    public Task<Result> Handle(ConfirmEmailCommand command, CancellationToken ct) =>
        auth.ConfirmEmailAsync(command.UserId, command.Token.Trim(), ct);
}