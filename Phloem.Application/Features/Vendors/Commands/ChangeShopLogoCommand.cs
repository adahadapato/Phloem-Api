using MediatR;
using Phloem.Application.Common.Caching;
using Phloem.Domain.Shared.Results;

namespace Phloem.Application.Features.Vendors.Commands;

/// <summary>Seller setup / Shop settings → logo. Returns the logo's URL so the app can show it at once.</summary>
[InvalidatesCache(CacheTags.Vendors, CacheTags.Dishes)]
public sealed record ChangeShopLogoCommand(Stream Content, long SizeBytes) : IRequest<Result<string>>;