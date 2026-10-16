using MediatR;
using Phloem.Domain.Shared.Results;

namespace Phloem.Application.Features.Auth.Commands;

/// <summary>From the reset link in the email: Email + Token come from the link.</summary>
public sealed record ResetPasswordCommand(string Email, string Token, string NewPassword, string ConfirmNewPassword)
    : IRequest<Result>;