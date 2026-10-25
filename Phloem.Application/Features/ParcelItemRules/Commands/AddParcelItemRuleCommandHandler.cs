using MediatR;
using Phloem.Domain.Entities;
using Phloem.Domain.Interfaces;
using Phloem.Domain.Shared.Results;

namespace Phloem.Application.Features.ParcelItemRules.Commands;

/// <summary>Handles <see cref="AddParcelItemRuleCommand"/>.</summary>
public sealed class AddParcelItemRuleCommandHandler(IParcelItemRuleRepository rules, IUnitOfWork unitOfWork)
    : IRequestHandler<AddParcelItemRuleCommand, Result<Guid>>
{
    /// <summary>Adds the line after the last one and saves.</summary>
    public async Task<Result<Guid>> Handle(AddParcelItemRuleCommand command, CancellationToken ct)
    {
        var next = await rules.GetLastSortOrderAsync(ct) + 1;
        var rule = ParcelItemRule.Create(command.Kind, command.Name, command.Detail, next);
        rules.Add(rule);
        await unitOfWork.SaveChangesAsync(ct);
        return rule.Id;
    }
}
