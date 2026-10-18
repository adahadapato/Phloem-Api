using MediatR;
using Phloem.Application.Common.Caching;
using Phloem.Application.Features.Vendors.Dtos;
using Phloem.Domain.Shared.Results;

namespace Phloem.Application.Features.Vendors.Queries;

/// <summary>A shop's page, as buyers see it. Only approved shops are found.</summary>
[Cacheable(CacheTags.Vendors, Minutes = 5)]
public sealed record GetShopQuery(Guid ShopId) : IRequest<Result<ShopDetailsDto>>;
