using MediatR;
using Phloem.Domain.Shared.Results;

namespace Phloem.Application.Features.Accounts.Commands;

/// <summary>Security → "Switch off two-factor". Needs the password. Not allowed for admins.</summary>
/// <param name="Password">The user's current password.</param>
public sealed record DisableTwoFactorCommand(string Password) : IRequest<Result>;