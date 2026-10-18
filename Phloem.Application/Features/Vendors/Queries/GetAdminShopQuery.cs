using MediatR;
using Phloem.Application.Features.Vendors.Dtos;
using Phloem.Domain.Shared.Results;

namespace Phloem.Application.Features.Vendors.Queries;

/// <summary>Admin → one shop with its documents (the review screen).</summary>
public sealed record GetAdminShopQuery(Guid ShopId) : IRequest<Result<AdminShopDetailsDto>>;
