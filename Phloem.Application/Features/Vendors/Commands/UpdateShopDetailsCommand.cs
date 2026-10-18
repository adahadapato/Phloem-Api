using MediatR;
using Phloem.Application.Common.Caching;
using Phloem.Domain.Shared.Results;

namespace Phloem.Application.Features.Vendors.Commands;

/// <summary>
/// Shop settings → name, business email and phone. Blank email / phone mean "use my login account's".
/// A rejected shop goes back to "waiting for approval" once its details are fixed.
/// </summary>
[InvalidatesCache(CacheTags.Vendors, CacheTags.Dishes)]
public sealed record UpdateShopDetailsCommand(string ShopName, string? BusinessEmail, string? BusinessPhone)
    : IRequest<Result>;