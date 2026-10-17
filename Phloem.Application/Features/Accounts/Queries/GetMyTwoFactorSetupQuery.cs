using MediatR;
using Phloem.Application.Features.Accounts.Dtos;
using Phloem.Domain.Shared.Results;

namespace Phloem.Application.Features.Accounts.Queries;

/// <summary>
/// Security → "Set up two-factor": the key / QR link for the authenticator app, for the SIGNED-IN
/// user only (never by Id: the key lets anyone generate valid codes). Never cached or logged.
/// </summary>
public sealed record GetMyTwoFactorSetupQuery : IRequest<Result<TwoFactorSetupDto>>;