using MediatR;
using Phloem.Application.Features.Accounts.Dtos;
using Phloem.Application.Features.Auth.Dtos;
using Phloem.Domain.Enums;
using Phloem.Domain.Shared.Results;

namespace Phloem.Application.Features.Auth.Commands;

/// <summary>
/// Register page. Signs straight in. Date of birth and home address are optional here (required later to open a shop
/// or send a parcel); a home address also becomes the first delivery address ("Home").
/// </summary>
public sealed record RegisterCommand(
    string FirstName,
    string LastName,
    string Email,
    string? PhoneNumber,
    Country Country,
    DateOnly? DateOfBirth,
    HomeAddressInput? HomeAddress,
    string Password,
    bool AgreedToTerms) : IRequest<Result<AuthTokensDto>>;