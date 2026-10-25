using MediatR;
using Phloem.Application.Common.Caching;
using Phloem.Application.Features.ParcelItemRules.Dtos;
using Phloem.Domain.Shared.Results;

namespace Phloem.Application.Features.ParcelItemRules.Queries;

/// <summary>
/// The "what can be sent" list, in display order: for the Send a parcel form, the Help page and the admin editor.
/// Public (read before signing in). Cached; cleared by every admin change to the list.
/// </summary>
[Cacheable(CacheTags.ParcelItemRules, Minutes = 60)]
public sealed record GetParcelItemRulesQuery : IRequest<Result<IReadOnlyList<ParcelItemRuleDto>>>;
