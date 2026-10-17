using MediatR;
using Phloem.Domain.Interfaces;
using Phloem.Domain.Shared.Errors;
using Phloem.Domain.Shared.Results;

namespace Phloem.Application.Features.ShipmentRoutes.Commands;

/// <summary>Handles <see cref="RemoveShipmentRateCommand"/>.</summary>
public sealed class RemoveShipmentRateCommandHandler(IShipmentRouteRepository routes, IUnitOfWork unitOfWork)
    : IRequestHandler<RemoveShipmentRateCommand, Result>
{
    /// <summary>Removes the rate (fine if it isn't there).</summary>
    /// <param name="command">The command.</param>
    /// <param name="ct">Cancels the database calls.</param>
    /// <returns>Success; or <see cref="ShipmentErrors.RouteNotFound"/>.</returns>
    public async Task<Result> Handle(RemoveShipmentRateCommand command, CancellationToken ct)
    {
        var route = await routes.GetByCountriesAsync(command.FromCountry, command.ToCountry, ct);
        if (route is null) return ShipmentErrors.RouteNotFound;

        route.RemoveRate(command.Speed);

        await unitOfWork.SaveChangesAsync(ct);
        return Result.Success();
    }
}
