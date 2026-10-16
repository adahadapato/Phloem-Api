using MediatR;
using Phloem.Domain.Interfaces;
using Phloem.Domain.Shared.Errors;
using Phloem.Domain.Shared.Results;

namespace Phloem.Application.Features.DeliveryPricings.Commands;

/// <summary>Handles <see cref="SetDeliveryWindowChargeCommand"/>.</summary>
public sealed class SetDeliveryWindowChargeCommandHandler(
    IDeliveryPricingRepository pricings,
    IUnitOfWork unitOfWork)
    : IRequestHandler<SetDeliveryWindowChargeCommand, Result>
{
    /// <summary>Sets the charge, or returns <see cref="DeliveryPricingErrors.NotSet"/> if the country has no pricing.</summary>
    public async Task<Result> Handle(SetDeliveryWindowChargeCommand command, CancellationToken ct)
    {
        var pricing = await pricings.GetByCountryAsync(command.Country, ct);
        if (pricing is null) return DeliveryPricingErrors.NotSet;

        pricing.SetWindowCharge(command.Window, command.ExtraCharge);

        await unitOfWork.SaveChangesAsync(ct);
        return Result.Success();
    }
}