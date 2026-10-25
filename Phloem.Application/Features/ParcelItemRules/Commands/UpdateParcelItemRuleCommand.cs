using MediatR;
using Phloem.Application.Common.Caching;
using Phloem.Domain.Enums;
using Phloem.Domain.Shared.Results;

namespace Phloem.Application.Features.ParcelItemRules.Commands;

/// <summary>Admin changes a line's wording or kind.</summary>
/// <param name="Id">The line.</param>
/// <param name="Kind">NotAllowed or Restricted.</param>
/// <param name="Name">e.g. "Fresh meat or fish".</param>
/// <param name="Detail">Optional condition or reason (empty removes it).</param>
[InvalidatesCache(CacheTags.ParcelItemRules)]
public sealed record UpdateParcelItemRuleCommand(Guid Id, ParcelItemRuleKind Kind, string Name, string? Detail)
    : IRequest<Result>;
