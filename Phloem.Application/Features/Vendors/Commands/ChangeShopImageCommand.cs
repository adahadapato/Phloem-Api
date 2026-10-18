using MediatR;
using Phloem.Application.Common.Caching;
using Phloem.Domain.Shared.Results;
using Phloem.Domain.Enums;

namespace Phloem.Application.Features.Vendors.Commands;

/// <summary>Seller setup / Shop settings → logo or cover photo. Returns its URL so the app can show it at once.</summary>
[InvalidatesCache(CacheTags.Vendors, CacheTags.Dishes)]
public sealed record ChangeShopImageCommand(ShopImage Image, Stream Content, long SizeBytes) : IRequest<Result<string>>;