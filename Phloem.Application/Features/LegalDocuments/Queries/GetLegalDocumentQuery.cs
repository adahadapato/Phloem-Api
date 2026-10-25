using MediatR;
using Phloem.Application.Common.Caching;
using Phloem.Application.Features.LegalDocuments.Dtos;
using Phloem.Domain.Enums;
using Phloem.Domain.Shared.Results;

namespace Phloem.Application.Features.LegalDocuments.Queries;

/// <summary>
/// The current Terms or Privacy Policy, for the website's and the app's legal pages. Public (read before signing up).
/// Cached; cleared when an admin publishes a new version.
/// </summary>
/// <param name="Type">Terms or Privacy.</param>
[Cacheable(CacheTags.LegalDocuments, Minutes = 60)]
public sealed record GetLegalDocumentQuery(LegalDocumentType Type) : IRequest<Result<LegalDocumentDto>>;
