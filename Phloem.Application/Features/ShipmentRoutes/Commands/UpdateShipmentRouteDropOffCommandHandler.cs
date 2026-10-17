using MediatR;
using Phloem.Domain.Interfaces;
using Phloem.Domain.Shared.Errors;
using Phloem.Domain.Shared.Results;

namespace Phloem.Application.Features.ShipmentRoutes.Commands;

/// <summary>Handles <see cref="UpdateShipmentRouteDropOffCommand"/>.</summary>
public sealed class UpdateShipmentRouteDropOffCommandHandler(IShipmentRouteRepository routes, IUnitOfWork unitOfWork)
    : IRequestHandler<UpdateShipmentRouteDropOffCommand, Result>
{
    /// <summary>Updates the hub details.</summary>
    /// <param name="command">The command.</param>
    /// <param name="ct">Cancels the database calls.</param>
    /// <returns>Success; or <see cref="ShipmentErrors.RouteNotFound"/>.</returns>
    public async Task<Result> Handle(UpdateShipmentRouteDropOffCommand command, CancellationToken ct)
    {
        var route = await routes.GetByCountriesAsync(command.FromCountry, command.ToCountry, ct);
        if (route is null) return ShipmentErrors.RouteNotFound;

        route.UpdateDropOff(command.DropOffAddress, command.DropOffHours);

        await unitOfWork.SaveChangesAsync(ct);
        return Result.Success();
    }
}
