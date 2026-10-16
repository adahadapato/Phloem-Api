using MediatR;
using Phloem.Domain.Enums;
using Phloem.Domain.Shared.Results;

namespace Phloem.Application.Features.Accounts.Commands;

/// <summary>Edit profile page. Blank phone = remove the phone number.</summary>
public sealed record UpdateProfileCommand(string FullName, Country Country, string? PhoneNumber)
    : IRequest<Result>;