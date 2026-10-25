using MediatR;
using Phloem.Domain.Enums;
using Phloem.Domain.Shared.Results;

namespace Phloem.Application.Features.Accounts.Commands;

/// <summary>Delivery address page → add new. Returns the new address Id.</summary>
/// <param name="Label">"Home", "Work", "Mum's house".</param>
/// <param name="FullAddress">The address as typed.</param>
/// <param name="Country">Its country.</param>
/// <param name="MakeDefault">Make it the primary (default) address.</param>
/// <param name="RecipientName">Who receives the food; empty = the account holder.</param>
/// <param name="PhoneNumber">Who the seller calls; empty = the account holder's phone.</param>
public sealed record AddAddressCommand(string Label, string FullAddress, Country Country, bool MakeDefault,
                                       string? RecipientName = null, string? PhoneNumber = null)
    : IRequest<Result<Guid>>;