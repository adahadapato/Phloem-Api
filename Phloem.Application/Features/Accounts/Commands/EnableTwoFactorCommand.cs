using MediatR;
using Phloem.Domain.Shared.Results;

namespace Phloem.Application.Features.Accounts.Commands;

/// <summary>
/// Security → "Set up two-factor" → enter the 6-digit code from the authenticator app.
/// Returns the recovery codes, which the app shows ONCE ("save these somewhere safe").
/// </summary>
/// <param name="Code">The current code from the authenticator app.</param>
public sealed record EnableTwoFactorCommand(string Code) : IRequest<Result<IReadOnlyCollection<string>>>;