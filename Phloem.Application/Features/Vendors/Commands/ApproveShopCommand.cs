using MediatR;
using Phloem.Application.Common.Caching;
using Phloem.Domain.Shared.Results;

namespace Phloem.Application.Features.Vendors.Commands;

/// <summary>Admin approves a shop: it goes live and its owner becomes a Seller.</summary>
[InvalidatesCache(CacheTags.Vendors, CacheTags.Dishes)]
public sealed record ApproveShopCommand(Guid ShopId) : IRequest<Result>;
