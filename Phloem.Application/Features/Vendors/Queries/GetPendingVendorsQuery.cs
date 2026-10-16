using MediatR;
using Phloem.Application.Features.Vendors.Dtos;
using Phloem.Domain.Shared.Results;

namespace Phloem.Application.Features.Vendors.Queries;

/// <summary>
/// Admin → Sellers tab → "Waiting for approval", oldest first, with each owner's name.
/// Not cached: admins need to see new shops at once.
/// </summary>
public sealed record GetPendingVendorsQuery : IRequest<Result<IReadOnlyList<AdminVendorDto>>>;