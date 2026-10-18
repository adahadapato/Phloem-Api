using MediatR;
using Phloem.Application.Features.Vendors.Dtos;
using Phloem.Domain.Interfaces;
using Phloem.Domain.Shared.Errors;
using Phloem.Domain.Shared.Results;

namespace Phloem.Application.Features.Vendors.Queries;

/// <summary>Loads the shop owned by the signed-in user; <see cref="VendorErrors.NotFound"/> if they have none.</summary>
public sealed class GetMyShopQueryHandler(
    ICurrentUser currentUser,
    IVendorRepository vendors,
    IFileStorage storage,
    TimeProvider time)
    : IRequestHandler<GetMyShopQuery, Result<MyShopDto>>
{
    /// <exception cref="InvalidOperationException">Called without a signed-in user (endpoint misconfigured).</exception>
    public async Task<Result<MyShopDto>> Handle(GetMyShopQuery query, CancellationToken ct)
    {
        var userId = currentUser.UserId
            ?? throw new InvalidOperationException("This action requires a signed-in user.");

        var vendor = await vendors.GetByOwnerIdAsync(userId, ct);
        if (vendor is null) return VendorErrors.NotFound;

        return MyShopDto.From(vendor, storage, time.GetUtcNow().UtcDateTime);
    }
}