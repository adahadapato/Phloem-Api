using MediatR;
using Phloem.Application.Common.Caching;
using Phloem.Domain.Shared.Results;

namespace Phloem.Application.Features.Vendors.Commands;

/// <summary>Admin blocks (hides) or unblocks a shop; the owner's Seller role follows.</summary>
[InvalidatesCache(CacheTags.Vendors, CacheTags.Dishes)]
public sealed record SetShopBlockedCommand(Guid ShopId, bool Blocked) : IRequest<Result>;
