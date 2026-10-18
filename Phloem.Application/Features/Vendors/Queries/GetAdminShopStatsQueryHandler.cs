using MediatR;
using Phloem.Application.Features.Vendors.Dtos;
using Phloem.Domain.Enums;
using Phloem.Domain.Interfaces;
using Phloem.Domain.Shared.Results;

namespace Phloem.Application.Features.Vendors.Queries;

public sealed class GetAdminShopStatsQueryHandler(IVendorRepository vendors)
    : IRequestHandler<GetAdminShopStatsQuery, Result<AdminShopStatsDto>>
{
    public async Task<Result<AdminShopStatsDto>> Handle(GetAdminShopStatsQuery query, CancellationToken ct)
    {
        var counts = await vendors.CountByStatusAsync(ct);
        int Count(VendorStatus status) => counts.GetValueOrDefault(status);

        var pending = Count(VendorStatus.PendingApproval);
        var approved = Count(VendorStatus.Approved);
        var rejected = Count(VendorStatus.Rejected);
        var blocked = Count(VendorStatus.Blocked);

        return new AdminShopStatsDto(pending, approved, rejected, blocked, pending + approved + rejected + blocked);
    }
}
