using MediatR;
using Phloem.Domain.Shared.Results;

namespace Phloem.Application.Features.Accounts.Commands;

public sealed record SetDefaultAddressCommand(Guid AddressId) : IRequest<Result>;