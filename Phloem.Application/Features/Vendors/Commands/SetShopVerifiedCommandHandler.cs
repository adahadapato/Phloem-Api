using MediatR;
using Phloem.Domain.Interfaces;
using Phloem.Domain.Shared.Errors;
using Phloem.Domain.Shared.Results;

namespace Phloem.Application.Features.Vendors.Commands;

public sealed class SetShopVerifiedCommandHandler(
    IVendorRepository vendors,
    IUnitOfWork unitOfWork,
    TimeProvider time)
    : IRequestHandler<SetShopVerifiedCommand, Result>
{
    public async Task<Result> Handle(SetShopVerifiedCommand command, CancellationToken ct)
    {
        var vendor = await vendors.GetByIdAsync(command.ShopId, ct);
        if (vendor is null) return VendorErrors.NotFound;

        if (command.Verified)
        {
            var verified = vendor.Verify(time.GetUtcNow().UtcDateTime);
            if (verified.IsFailure) return verified;
        }
        else
        {
            vendor.Unverify();
        }

        await unitOfWork.SaveChangesAsync(ct);
        return Result.Success();
    }
}
