using MediatR;
using Phloem.Domain.Entities;
using Phloem.Domain.Interfaces;
using Phloem.Domain.Shared.Results;

namespace Phloem.Application.Features.LegalDocuments.Commands;

/// <summary>Handles <see cref="PublishLegalDocumentCommand"/>.</summary>
public sealed class PublishLegalDocumentCommandHandler(
    ILegalDocumentRepository documents,
    IUnitOfWork unitOfWork,
    TimeProvider time)
    : IRequestHandler<PublishLegalDocumentCommand, Result<int>>
{
    /// <summary>Adds the next version of the type, effective now, and saves.</summary>
    /// <remarks>The unique (Type, Version) index stops two admins publishing the same number at once.</remarks>
    public async Task<Result<int>> Handle(PublishLegalDocumentCommand command, CancellationToken ct)
    {
        var next = await documents.GetLatestVersionAsync(command.Type, ct) + 1;
        var document = LegalDocument.Publish(command.Type, next, command.Title, command.Content, command.IsDraft,
                                             time.GetUtcNow().UtcDateTime);
        documents.Add(document);
        await unitOfWork.SaveChangesAsync(ct);
        return next;
    }
}
