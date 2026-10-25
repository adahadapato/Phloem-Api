using MediatR;
using Phloem.Domain.Enums;
using Phloem.Domain.Shared.Results;

namespace Phloem.Application.Features.Accounts.Commands;

/// <summary>AddressId comes from the URL (PUT /me/addresses/{id}).</summary>
/// <param name="RecipientName">Who receives the food; empty = the account holder.</param>
/// <param name="PhoneNumber">Who the seller calls; empty = the account holder's phone.</param>
public sealed record UpdateAddressCommand(Guid AddressId, string Label, string FullAddress, Country Country,
                                          string? RecipientName = null, string? PhoneNumber = null)
    : IRequest<Result>;