using MediatR;
using Phloem.Application.Common.Caching;
using Phloem.Domain.Enums;
using Phloem.Domain.Shared.Results;

namespace Phloem.Application.Features.Vendors.Commands;


/// <summary>Shop settings → remove the logo or the cover photo. Removing one that isn't there is fine.</summary>
[InvalidatesCache(CacheTags.Vendors, CacheTags.Dishes)]
public sealed record RemoveShopImageCommand(ShopImage Image) : IRequest<Result>;