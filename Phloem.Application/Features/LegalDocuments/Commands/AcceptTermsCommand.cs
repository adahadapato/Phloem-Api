using MediatR;
using Phloem.Application.Common.Terms;
using Phloem.Domain.Enums;
using Phloem.Domain.Interfaces;
using Phloem.Domain.Shared.Errors;
using Phloem.Domain.Shared.Results;

namespace Phloem.Application.Features.LegalDocuments.Commands;

/// <summary>
/// "We've updated our Terms" → Accept. <paramref name="Version"/> is the version the person was shown: if a newer one
/// was published meanwhile, they're asked to read that one instead (Terms.Outdated).
/// </summary>
/// <param name="Version">The version on screen.</param>
public sealed record AcceptTermsCommand(int Version) : IRequest<Result>;

/// <summary>Handles <see cref="AcceptTermsCommand"/>.</summary>
public sealed class AcceptTermsCommandHandler(
    ICurrentUser currentUser,
    ILegalDocumentRepository documents,
    TermsGate terms,
    IUnitOfWork unitOfWork)
    : IRequestHandler<AcceptTermsCommand, Result>
{
    /// <exception cref="InvalidOperationException">Called without a signed-in user (endpoint misconfigured).</exception>
    public async Task<Result> Handle(AcceptTermsCommand command, CancellationToken ct)
    {
        var userId = currentUser.UserId
            ?? throw new InvalidOperationException("This action requires a signed-in user.");

        var current = await documents.GetLatestVersionAsync(LegalDocumentType.Terms, ct);
        if (command.Version != current) return TermsErrors.Outdated;

        await terms.RecordAsync(userId, LegalDocumentType.Terms, AcceptanceContext.UpdatedTerms, null, ct);
        await unitOfWork.SaveChangesAsync(ct);
        return Result.Success();
    }
}

/// <summary>"We've updated our Privacy Policy" → Got it. Records that the person was shown that version.</summary>
/// <param name="Version">The version on screen.</param>
public sealed record AcknowledgePrivacyCommand(int Version) : IRequest<Result>;

/// <summary>Handles <see cref="AcknowledgePrivacyCommand"/>.</summary>
public sealed class AcknowledgePrivacyCommandHandler(
    ICurrentUser currentUser,
    ILegalDocumentRepository documents,
    TermsGate terms,
    IUnitOfWork unitOfWork)
    : IRequestHandler<AcknowledgePrivacyCommand, Result>
{
    /// <remarks>An older version is simply ignored (a newer notice will show).</remarks>
    /// <exception cref="InvalidOperationException">Called without a signed-in user (endpoint misconfigured).</exception>
    public async Task<Result> Handle(AcknowledgePrivacyCommand command, CancellationToken ct)
    {
        var userId = currentUser.UserId
            ?? throw new InvalidOperationException("This action requires a signed-in user.");

        var current = await documents.GetLatestVersionAsync(LegalDocumentType.Privacy, ct);
        if (command.Version != current) return Result.Success();

        await terms.RecordAsync(userId, LegalDocumentType.Privacy, AcceptanceContext.PrivacyNotice, null, ct);
        await unitOfWork.SaveChangesAsync(ct);
        return Result.Success();
    }
}
