using MediatR;
using Phloem.Domain.Interfaces;
using Phloem.Domain.Shared.Errors;
using Phloem.Domain.Shared.Results;

namespace Phloem.Application.Features.ParcelItemRules.Commands;

/// <summary>Handles <see cref="UpdateParcelItemRuleCommand"/>.</summary>
public sealed class UpdateParcelItemRuleCommandHandler(IParcelItemRuleRepository rules, IUnitOfWork unitOfWork)
    : IRequestHandler<UpdateParcelItemRuleCommand, Result>
{
    /// <summary>Updates the line.</summary>
    /// <returns>Success; or <see cref="ParcelItemRuleErrors.NotFound"/>.</returns>
    public async Task<Result> Handle(UpdateParcelItemRuleCommand command, CancellationToken ct)
    {
        var rule = await rules.GetByIdAsync(command.Id, ct);
        if (rule is null) return ParcelItemRuleErrors.NotFound;

        rule.Update(command.Kind, command.Name, command.Detail);
        await unitOfWork.SaveChangesAsync(ct);
        return Result.Success();
    }
}
