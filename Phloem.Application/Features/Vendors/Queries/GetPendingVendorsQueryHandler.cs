using MediatR;
using Phloem.Application.Features.Vendors.Dtos;
using Phloem.Domain.Interfaces;
using Phloem.Domain.Shared.Results;

namespace Phloem.Application.Features.Vendors.Queries;

public sealed class GetPendingVendorsQueryHandler(
    IVendorRepository vendors,
    IUserProfileRepository profiles)
    : IRequestHandler<GetPendingVendorsQuery, Result<IReadOnlyList<AdminVendorDto>>>
{
    private const string UnknownUser = "Unknown user";

    public async Task<Result<IReadOnlyList<AdminVendorDto>>> Handle(GetPendingVendorsQuery query, CancellationToken ct)
    {
        var pending = await vendors.GetPendingApprovalAsync(ct);

        var owners = await profiles.GetFullNamesAsync(pending.Select(v => v.OwnerId).Distinct().ToList(), ct);

        IReadOnlyList<AdminVendorDto> items = pending
            .Select(v => AdminVendorDto.From(v, owners.GetValueOrDefault(v.OwnerId, UnknownUser)))
            .ToList();

        return Result<IReadOnlyList<AdminVendorDto>>.Success(items);
    }
}