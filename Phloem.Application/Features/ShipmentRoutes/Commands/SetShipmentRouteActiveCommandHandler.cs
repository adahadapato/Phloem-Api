using MediatR;
using Phloem.Domain.Interfaces;
using Phloem.Domain.Shared.Errors;
using Phloem.Domain.Shared.Results;

namespace Phloem.Application.Features.ShipmentRoutes.Commands;

/// <summary>Handles <see cref="SetShipmentRouteActiveCommand"/>.</summary>
public sealed class SetShipmentRouteActiveCommandHandler(IShipmentRouteRepository routes, IUnitOfWork unitOfWork)
    : IRequestHandler<SetShipmentRouteActiveCommand, Result>
{
    /// <summary>Switches the route on or off.</summary>
    /// <param name="command">The command.</param>
    /// <param name="ct">Cancels the database calls.</param>
    /// <returns>
    /// Success; or <see cref="ShipmentErrors.RouteNotFound"/>,
    /// <see cref="ShipmentErrors.RouteHasNoRates"/> (switching on with no prices).
    /// </returns>
    public async Task<Result> Handle(SetShipmentRouteActiveCommand command, CancellationToken ct)
    {
        var route = await routes.GetByCountriesAsync(command.FromCountry, command.ToCountry, ct);
        if (route is null) return ShipmentErrors.RouteNotFound;

        if (command.IsActive)
        {
            var activated = route.Activate();
            if (activated.IsFailure) return activated;
        }
        else
        {
            route.Deactivate();
        }

        await unitOfWork.SaveChangesAsync(ct);
        return Result.Success();
    }
}
