using MediatR;
using Phloem.Domain.Entities;
using Phloem.Domain.Interfaces;
using Phloem.Domain.Shared.Results;

namespace Phloem.Application.Features.Commissions.Commands;

/// <summary>Handles <see cref="SetCommissionCommand"/>.</summary>
public sealed class SetCommissionCommandHandler(ICommissionRepository commissions, IUnitOfWork unitOfWork)
    : IRequestHandler<SetCommissionCommand, Result>
{
    /// <summary>Creates or changes the country's rate and saves.</summary>
    public async Task<Result> Handle(SetCommissionCommand command, CancellationToken ct)
    {
        var commission = await commissions.GetByCountryAsync(command.Country, ct);

        if (commission is null)
            commissions.Add(Commission.Create(command.Country, command.RatePercent));
        else
            commission.ChangeRate(command.RatePercent);

        await unitOfWork.SaveChangesAsync(ct);
        return Result.Success();
    }
}