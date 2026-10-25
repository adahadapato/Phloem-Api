using MediatR;
using Phloem.Application.Common.Caching;
using Phloem.Domain.Enums;
using Phloem.Domain.Shared.Results;

namespace Phloem.Application.Features.ParcelItemRules.Commands;

/// <summary>Admin adds a line at the end of the "what can be sent" list. Returns the new line's id.</summary>
/// <param name="Kind">NotAllowed or Restricted.</param>
/// <param name="Name">e.g. "Fresh meat or fish".</param>
/// <param name="Detail">Optional condition or reason.</param>
[InvalidatesCache(CacheTags.ParcelItemRules)]
public sealed record AddParcelItemRuleCommand(ParcelItemRuleKind Kind, string Name, string? Detail)
    : IRequest<Result<Guid>>;
