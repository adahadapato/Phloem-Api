using MediatR;
using Phloem.Application.Common.Caching;
using Phloem.Domain.Enums;
using Phloem.Domain.Shared.Results;

namespace Phloem.Application.Features.LegalDocuments.Commands;

/// <summary>
/// Admin publishes a new version of the Terms or Privacy Policy (e.g. the lawyer-reviewed wording replacing the
/// temporary draft). The old versions stay, unchanged. Returns the new version number.
/// </summary>
/// <param name="Type">Terms or Privacy.</param>
/// <param name="Title">e.g. "Terms &amp; Conditions".</param>
/// <param name="Content">The full text, in Markdown.</param>
/// <param name="IsDraft">True if this wording is still temporary.</param>
[InvalidatesCache(CacheTags.LegalDocuments)]
public sealed record PublishLegalDocumentCommand(LegalDocumentType Type, string Title, string Content, bool IsDraft)
    : IRequest<Result<int>>;
