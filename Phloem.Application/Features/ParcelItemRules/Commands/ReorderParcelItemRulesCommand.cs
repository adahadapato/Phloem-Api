using MediatR;
using Phloem.Application.Common.Caching;
using Phloem.Domain.Shared.Results;

namespace Phloem.Application.Features.ParcelItemRules.Commands;

/// <summary>Admin puts the list in a new order: every line's id, in the order they should show.</summary>
/// <param name="Ids">Every line exactly once, first to last.</param>
[InvalidatesCache(CacheTags.ParcelItemRules)]
public sealed record ReorderParcelItemRulesCommand(IReadOnlyList<Guid> Ids) : IRequest<Result>;
