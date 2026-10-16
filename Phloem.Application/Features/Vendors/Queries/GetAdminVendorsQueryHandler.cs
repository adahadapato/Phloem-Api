using MediatR;
using Phloem.Application.Features.Vendors.Dtos;
using Phloem.Domain.Interfaces;
using Phloem.Domain.Shared.Results;

namespace Phloem.Application.Features.Vendors.Queries;

public sealed class GetAdminVendorsQueryHandler(
    IVendorRepository vendors,
    IUserProfileRepository profiles)
    : IRequestHandler<GetAdminVendorsQuery, Result<PagedResult<AdminVendorDto>>>
{
    private const string UnknownUser = "Unknown user";

    public async Task<Result<PagedResult<AdminVendorDto>>> Handle(GetAdminVendorsQuery query, CancellationToken ct)
    {
        var page = new PageRequest { Page = query.Page, PageSize = query.PageSize };
        var paged = await vendors.GetPagedAsync(query.Status, query.Country, query.Search, page, ct);

        var owners = await profiles.GetFullNamesAsync(paged.Items.Select(v => v.OwnerId).Distinct().ToList(), ct);

        var items = paged.Items
            .Select(v => AdminVendorDto.From(v, owners.GetValueOrDefault(v.OwnerId, UnknownUser)))
            .ToList();

        return new PagedResult<AdminVendorDto>(items, paged.TotalCount, paged.Page, paged.PageSize);
    }
}