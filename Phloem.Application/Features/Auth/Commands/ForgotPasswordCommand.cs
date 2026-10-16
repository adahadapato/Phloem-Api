using MediatR;
using Phloem.Domain.Shared.Results;

namespace Phloem.Application.Features.Auth.Commands;

/// <summary>Login → "Forgot password?". Always succeeds, so nobody can probe which emails exist.</summary>
public sealed record ForgotPasswordCommand(string Email) : IRequest<Result>;