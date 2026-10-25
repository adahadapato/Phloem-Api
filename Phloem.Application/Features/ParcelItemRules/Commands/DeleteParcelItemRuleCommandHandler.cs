using MediatR;
using Phloem.Domain.Interfaces;
using Phloem.Domain.Shared.Errors;
using Phloem.Domain.Shared.Results;

namespace Phloem.Application.Features.ParcelItemRules.Commands;

/// <summary>Handles <see cref="DeleteParcelItemRuleCommand"/>.</summary>
public sealed class DeleteParcelItemRuleCommandHandler(IParcelItemRuleRepository rules, IUnitOfWork unitOfWork)
    : IRequestHandler<DeleteParcelItemRuleCommand, Result>
{
    /// <summary>Deletes the line (a real delete: nothing points at it).</summary>
    /// <returns>Success; or <see cref="ParcelItemRuleErrors.NotFound"/>.</returns>
    public async Task<Result> Handle(DeleteParcelItemRuleCommand command, CancellationToken ct)
    {
        var rule = await rules.GetByIdAsync(command.Id, ct);
        if (rule is null) return ParcelItemRuleErrors.NotFound;

        rules.Remove(rule);
        await unitOfWork.SaveChangesAsync(ct);
        return Result.Success();
    }
}
