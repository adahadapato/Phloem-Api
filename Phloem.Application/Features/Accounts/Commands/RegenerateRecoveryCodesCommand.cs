using MediatR;
using Phloem.Domain.Shared.Results;

namespace Phloem.Application.Features.Accounts.Commands;

/// <summary>
/// Security → "New recovery codes": the old ones stop working. Needs the password.
/// The app shows the new codes ONCE.
/// </summary>
/// <param name="Password">The user's current password.</param>
public sealed record RegenerateRecoveryCodesCommand(string Password) : IRequest<Result<IReadOnlyCollection<string>>>;