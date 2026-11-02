using MediatR;
using Phloem.Application.Features.Vendors.Dtos;
using Phloem.Domain.Interfaces;
using Phloem.Domain.Shared.Results;

namespace Phloem.Application.Features.Vendors.Queries;

public sealed class GetPendingShopsQueryHandler(
    ICurrentUser currentUser,
    IVendorRepository vendors,
    IUserProfileRepository profiles,
    IFileStorage storage)
    : IRequestHandler<GetPendingShopsQuery, Result<IReadOnlyList<AdminShopDto>>>
{
    private const string UnknownUser = "Unknown user";

    public async Task<Result<IReadOnlyList<AdminShopDto>>> Handle(GetPendingShopsQuery query, CancellationToken ct)
    {
        // A short list, so the country filter is done here
        var pending = (await vendors.GetPendingApprovalAsync(ct))
            .Where(v => currentUser.CoversCountry(v.Country))
            .ToList();

        var owners = await profiles.GetFullNamesAsync(pending.Select(v => v.OwnerId).Distinct().ToList(), ct);
        IReadOnlyList<AdminShopDto> items = pending
            .Select(v => AdminShopDto.From(v, owners.GetValueOrDefault(v.OwnerId, UnknownUser), storage))
            .ToList();

        return Result<IReadOnlyList<AdminShopDto>>.Success(items);
    }
}
