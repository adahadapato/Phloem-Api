using MediatR;
using Phloem.Domain.Interfaces;
using Phloem.Domain.Shared.Errors;
using Phloem.Domain.Shared.Results;

namespace Phloem.Application.Features.DeliveryPricings.Commands;

/// <summary>Handles <see cref="SetMyDeliveryFeeCommand"/>.</summary>
public sealed class SetMyDeliveryFeeCommandHandler(
    ICurrentUser currentUser,
    IVendorRepository vendors,
    IDeliveryPricingRepository pricings,
    IUnitOfWork unitOfWork)
    : IRequestHandler<SetMyDeliveryFeeCommand, Result>
{
    /// <summary>
    /// Sets the fee; or <see cref="VendorErrors.NotFound"/>, <see cref="DeliveryPricingErrors.NotSet"/>,
    /// <see cref="VendorErrors.Blocked"/>, <see cref="DeliveryPricingErrors.ShopFeeOutOfRange"/>.
    /// </summary>
    /// <exception cref="InvalidOperationException">Called without a signed-in user (endpoint misconfigured).</exception>
    public async Task<Result> Handle(SetMyDeliveryFeeCommand command, CancellationToken ct)
    {
        var userId = currentUser.UserId
            ?? throw new InvalidOperationException("This action requires a signed-in user.");

        var vendor = await vendors.GetByOwnerIdAsync(userId, ct);
        if (vendor is null) return VendorErrors.NotFound;

        var pricing = await pricings.GetByCountryAsync(vendor.Country, ct);
        if (pricing is null) return DeliveryPricingErrors.NotSet;

        var set = vendor.SetDeliveryFee(command.Fee, pricing);
        if (set.IsFailure) return set;

        await unitOfWork.SaveChangesAsync(ct);
        return Result.Success();
    }
}