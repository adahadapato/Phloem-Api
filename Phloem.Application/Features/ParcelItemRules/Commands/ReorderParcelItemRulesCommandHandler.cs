using MediatR;
using Phloem.Domain.Interfaces;
using Phloem.Domain.Shared.Errors;
using Phloem.Domain.Shared.Results;

namespace Phloem.Application.Features.ParcelItemRules.Commands;

/// <summary>Handles <see cref="ReorderParcelItemRulesCommand"/>.</summary>
public sealed class ReorderParcelItemRulesCommandHandler(IParcelItemRuleRepository rules, IUnitOfWork unitOfWork)
    : IRequestHandler<ReorderParcelItemRulesCommand, Result>
{
    /// <summary>Gives each line its new position (0, 1, 2…).</summary>
    /// <returns>Success; or <see cref="ParcelItemRuleErrors.OrderMismatch"/> when the ids aren't exactly the
    /// current lines (someone else added or deleted one meanwhile).</returns>
    public async Task<Result> Handle(ReorderParcelItemRulesCommand command, CancellationToken ct)
    {
        var all = await rules.GetAllForUpdateAsync(ct);
        if (all.Count != command.Ids.Count || !all.Select(r => r.Id).ToHashSet().SetEquals(command.Ids))
            return ParcelItemRuleErrors.OrderMismatch;

        var byId = all.ToDictionary(r => r.Id);
        for (var i = 0; i < command.Ids.Count; i++)
            byId[command.Ids[i]].MoveTo(i);

        await unitOfWork.SaveChangesAsync(ct);
        return Result.Success();
    }
}
