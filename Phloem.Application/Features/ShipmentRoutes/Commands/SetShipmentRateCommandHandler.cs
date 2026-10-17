using MediatR;
using Phloem.Domain.Interfaces;
using Phloem.Domain.Shared.Errors;
using Phloem.Domain.Shared.Results;

namespace Phloem.Application.Features.ShipmentRoutes.Commands;

/// <summary>Handles <see cref="SetShipmentRateCommand"/>.</summary>
public sealed class SetShipmentRateCommandHandler(IShipmentRouteRepository routes, IUnitOfWork unitOfWork)
    : IRequestHandler<SetShipmentRateCommand, Result>
{
    /// <summary>Sets the rate through the route (the aggregate root).</summary>
    /// <param name="command">The command.</param>
    /// <param name="ct">Cancels the database calls.</param>
    /// <returns>Success; or <see cref="ShipmentErrors.RouteNotFound"/>.</returns>
    public async Task<Result> Handle(SetShipmentRateCommand command, CancellationToken ct)
    {
        var route = await routes.GetByCountriesAsync(command.FromCountry, command.ToCountry, ct);
        if (route is null) return ShipmentErrors.RouteNotFound;

        route.SetRate(command.Speed, command.BaseCharge, command.PricePerKg,
                      command.MinTransitDays, command.MaxTransitDays);

        await unitOfWork.SaveChangesAsync(ct);
        return Result.Success();
    }
}
