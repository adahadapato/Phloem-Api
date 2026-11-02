using MediatR;
using Phloem.Application.Common.Caching;
using Phloem.Application.Features.LegalDocuments.Dtos;
using Phloem.Domain.Enums;
using Phloem.Domain.Shared.Results;

namespace Phloem.Application.Features.LegalDocuments.Commands;

/// <summary>
/// Admin → Terms &amp; Help: publish a new version. Help is published at once; Terms and Privacy wait for another
/// owner's approval (Approvals), then this same command is applied for them.
/// </summary>
[InvalidatesCache(CacheTags.LegalDocuments)]
public sealed record PublishLegalDocumentCommand(LegalDocumentType Type, string Title, string Content, bool IsDraft)
    : IRequest<Result<LegalPublishResultDto>>;
