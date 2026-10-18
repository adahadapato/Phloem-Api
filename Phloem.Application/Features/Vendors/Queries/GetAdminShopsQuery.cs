using MediatR;
using Phloem.Application.Features.Vendors.Dtos;
using Phloem.Domain.Enums;
using Phloem.Domain.Shared.Results;

namespace Phloem.Application.Features.Vendors.Queries;

/// <summary>
/// Admin → Shops tab → all shops, with each owner's name.
/// Optional filters: status (null = every status), country, name search.
/// </summary>
public sealed record GetAdminShopsQuery(
    VendorStatus? Status,
    Country? Country,
    string? Search,
    int Page = 1,
    int PageSize = PageRequest.DefaultPageSize) : IRequest<Result<PagedResult<AdminShopDto>>>;
