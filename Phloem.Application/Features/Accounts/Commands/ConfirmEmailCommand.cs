using MediatR;
using Phloem.Domain.Shared.Results;

namespace Phloem.Application.Features.Auth.Commands;

/// <summary>From the "confirm your email" link: UserId + Token come from the link's query string.</summary>
public sealed record ConfirmEmailCommand(Guid UserId, string Token) : IRequest<Result>;