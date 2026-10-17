using MediatR;
using Phloem.Domain.Entities;
using Phloem.Domain.Interfaces;
using Phloem.Domain.Shared.Errors;
using Phloem.Domain.Shared.Results;

namespace Phloem.Application.Features.ShipmentRoutes.Commands;

/// <summary>Handles <see cref="CreateShipmentRouteCommand"/>.</summary>
public sealed class CreateShipmentRouteCommandHandler(IShipmentRouteRepository routes, IUnitOfWork unitOfWork)
    : IRequestHandler<CreateShipmentRouteCommand, Result>
{
    /// <summary>Creates the route (switched off).</summary>
    /// <param name="command">The command.</param>
    /// <param name="ct">Cancels the database calls.</param>
    /// <returns>Success; or <see cref="ShipmentErrors.RouteAlreadyExists"/>.</returns>
    public async Task<Result> Handle(CreateShipmentRouteCommand command, CancellationToken ct)
    {
        if (await routes.GetByCountriesAsync(command.FromCountry, command.ToCountry, ct) is not null)
            return ShipmentErrors.RouteAlreadyExists;

        routes.Add(ShipmentRoute.Create(command.FromCountry, command.ToCountry,
                                        command.DropOffAddress, command.DropOffHours));

        await unitOfWork.SaveChangesAsync(ct);
        return Result.Success();
    }
}
