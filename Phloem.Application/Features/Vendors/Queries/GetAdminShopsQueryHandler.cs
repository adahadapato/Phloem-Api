using MediatR;
using Phloem.Application.Features.Vendors.Dtos;
using Phloem.Domain.Interfaces;
using Phloem.Domain.Shared.Results;
using Phloem.Application.Common.Authorization;

namespace Phloem.Application.Features.Vendors.Queries;

/// <summary>One page of shops, then the owners' names in ONE extra query (not one per shop).</summary>
public sealed class GetAdminShopsQueryHandler(
    ICurrentUser currentUser,
    IVendorRepository vendors,
    IUserProfileRepository profiles,
    IFileStorage storage)
    : IRequestHandler<GetAdminShopsQuery, Result<PagedResult<AdminShopDto>>>
{
    private const string UnknownUser = "Unknown user";

    public async Task<Result<PagedResult<AdminShopDto>>> Handle(GetAdminShopsQuery query, CancellationToken ct)
    {
        var page = new PageRequest { Page = query.Page, PageSize = query.PageSize };
        var paged = await vendors.GetPagedAsync(query.Status, query.Country, query.Search, page, ct,
                                                coveredCountries: currentUser.CountryLimit());

        var owners = await profiles.GetFullNamesAsync(paged.Items.Select(v => v.OwnerId).Distinct().ToList(), ct);
        var items = paged.Items
            .Select(v => AdminShopDto.From(v, owners.GetValueOrDefault(v.OwnerId, UnknownUser), storage))
            .ToList();

        return new PagedResult<AdminShopDto>(items, paged.TotalCount, paged.Page, paged.PageSize);
    }
}
