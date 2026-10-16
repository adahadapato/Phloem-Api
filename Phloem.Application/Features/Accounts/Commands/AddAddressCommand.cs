using MediatR;
using Phloem.Domain.Enums;
using Phloem.Domain.Shared.Results;

namespace Phloem.Application.Features.Accounts.Commands;

/// <summary>Delivery address page → add new. Returns the new address Id.</summary>
public sealed record AddAddressCommand(string Label, string FullAddress, Country Country, bool MakeDefault)
    : IRequest<Result<Guid>>;