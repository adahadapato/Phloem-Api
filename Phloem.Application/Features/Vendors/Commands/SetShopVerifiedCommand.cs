using MediatR;
using Phloem.Application.Common.Caching;
using Phloem.Domain.Shared.Results;

namespace Phloem.Application.Features.Vendors.Commands;

/// <summary>Admin gives or removes the ✓ badge (giving it needs every required document accepted).</summary>
[InvalidatesCache(CacheTags.Vendors, CacheTags.Dishes)]
public sealed record SetShopVerifiedCommand(Guid ShopId, bool Verified) : IRequest<Result>;
