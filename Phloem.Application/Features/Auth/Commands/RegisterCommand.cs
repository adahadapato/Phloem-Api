using MediatR;
using Phloem.Application.Features.Auth.Dtos;
using Phloem.Domain.Enums;
using Phloem.Domain.Shared.Results;

namespace Phloem.Application.Features.Auth.Commands;

/// <summary>Register page. Address optional → first default delivery address. Signs straight in.</summary>
public sealed record RegisterCommand(
    string FullName,
    string Email,
    string? PhoneNumber,
    string? Address,
    Country Country,
    string Password,
    bool AgreedToTerms) : IRequest<Result<AuthTokensDto>>;