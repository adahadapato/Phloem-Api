using MediatR;
using Phloem.Domain.Interfaces;
using Phloem.Domain.Shared.Errors;
using Phloem.Domain.Shared.Results;

namespace Phloem.Application.Features.DeliveryPricings.Commands;

/// <summary>Handles <see cref="RemoveDeliveryWindowCommand"/>.</summary>
public sealed class RemoveDeliveryWindowCommandHandler(
    IDeliveryPricingRepository pricings,
    IUnitOfWork unitOfWork)
    : IRequestHandler<RemoveDeliveryWindowCommand, Result>
{
    /// <summary>Removes the window (removing one that isn't offered is fine); NotSet if the country has no pricing.</summary>
    public async Task<Result> Handle(RemoveDeliveryWindowCommand command, CancellationToken ct)
    {
        var pricing = await pricings.GetByCountryAsync(command.Country, ct);
        if (pricing is null) return DeliveryPricingErrors.NotSet;

        pricing.RemoveWindow(command.Window);

        await unitOfWork.SaveChangesAsync(ct);
        return Result.Success();
    }
}