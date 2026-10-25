using MediatR;
using Phloem.Application.Common.Caching;
using Phloem.Domain.Shared.Results;

namespace Phloem.Application.Features.ParcelItemRules.Commands;

/// <summary>Admin removes a line from the list.</summary>
/// <param name="Id">The line.</param>
[InvalidatesCache(CacheTags.ParcelItemRules)]
public sealed record DeleteParcelItemRuleCommand(Guid Id) : IRequest<Result>;
