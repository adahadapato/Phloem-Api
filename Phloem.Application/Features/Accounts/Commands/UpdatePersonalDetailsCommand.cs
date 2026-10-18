using MediatR;
using Phloem.Application.Features.Accounts.Dtos;
using Phloem.Domain.Shared.Results;

namespace Phloem.Application.Features.Accounts.Commands;

/// <summary>Personal-details page: date of birth and home address, saved together.</summary>
public sealed record UpdatePersonalDetailsCommand(DateOnly DateOfBirth, HomeAddressInput HomeAddress)
    : IRequest<Result>;