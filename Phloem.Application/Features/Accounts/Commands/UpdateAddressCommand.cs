using MediatR;
using Phloem.Domain.Enums;
using Phloem.Domain.Shared.Results;

namespace Phloem.Application.Features.Accounts.Commands;

/// <summary>AddressId comes from the URL (PUT /me/addresses/{id}).</summary>
public sealed record UpdateAddressCommand(Guid AddressId, string Label, string FullAddress, Country Country)
    : IRequest<Result>;