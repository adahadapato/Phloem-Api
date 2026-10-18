using MediatR;
using Phloem.Application.Common.Caching;
using Phloem.Domain.Shared.Results;

namespace Phloem.Application.Features.Vendors.Commands;

/// <summary>Shop settings → cover photo (the banner on the shop page). Returns its URL.</summary>
[InvalidatesCache(CacheTags.Vendors, CacheTags.Dishes)]
public sealed record ChangeShopCoverCommand(Stream Content, long SizeBytes) : IRequest<Result<string>>;