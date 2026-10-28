using Phloem.Domain.Enums;

namespace Phloem.Domain.Entities;

/// <summary>
/// A record that someone accepted a version of the Terms (or was shown a version of the Privacy Policy): who, which
/// version, where (sign-up, opening a shop, a parcel, an order, the "updated Terms" screen) and when
/// (<see cref="Entity{TId}.CreatedAt"/>). Never changed or deleted: it's the proof.
/// </summary>
/// <remarks>
/// <para>The latest accepted Terms version decides whether someone may order, send parcels or sell (see
/// <c>TermsGate</c>). Rows are added, never updated, so the history stays complete.</para>
/// <para><see cref="UserId"/> isn't a foreign key: the record stays even if the account is later deleted.</para>
/// </remarks>
public class TermsAcceptance : Entity<Guid>
{
    /// <summary>Longest reference (order or tracking number).</summary>
    public const int ReferenceMaxLength = 40;

    /// <summary>Required by EF Core.</summary>
    protected TermsAcceptance() { }

    private TermsAcceptance(Guid id) : base(id) { }

    /// <summary>Who.</summary>
    public Guid UserId { get; private set; }

    /// <summary>Terms or Privacy (never Help).</summary>
    public LegalDocumentType DocumentType { get; private set; }

    /// <summary>The version they accepted or were shown.</summary>
    public int Version { get; private set; }

    /// <summary>Where it happened.</summary>
    public AcceptanceContext Context { get; private set; }

    /// <summary>The order or tracking number it came with, if any.</summary>
    public string? Reference { get; private set; }

    /// <summary>A new acceptance.</summary>
    /// <param name="userId">Who.</param>
    /// <param name="documentType">Terms or Privacy.</param>
    /// <param name="version">1 or more.</param>
    /// <param name="context">Where.</param>
    /// <param name="reference">Optional order or tracking number (at most <see cref="ReferenceMaxLength"/> characters).</param>
    /// <exception cref="ArgumentException">An empty user, Help as the document, or a reference that's too long.</exception>
    /// <exception cref="ArgumentOutOfRangeException">A version below 1, or an unknown document or context.</exception>
    public static TermsAcceptance Create(Guid userId, LegalDocumentType documentType, int version,
                                         AcceptanceContext context, string? reference = null)
    {
        if (userId == Guid.Empty) throw new ArgumentException("A user is required.", nameof(userId));
        if (!Enum.IsDefined(documentType)) throw new ArgumentOutOfRangeException(nameof(documentType));
        if (documentType == LegalDocumentType.Help)
            throw new ArgumentException("Help isn't accepted, only read.", nameof(documentType));
        if (!Enum.IsDefined(context)) throw new ArgumentOutOfRangeException(nameof(context));
        ArgumentOutOfRangeException.ThrowIfLessThan(version, 1);

        var cleanReference = string.IsNullOrWhiteSpace(reference) ? null : reference.Trim();
        if (cleanReference is { Length: > ReferenceMaxLength })
            throw new ArgumentException("The reference is too long.", nameof(reference));

        return new TermsAcceptance(Guid.CreateVersion7())
        {
            UserId = userId,
            DocumentType = documentType,
            Version = version,
            Context = context,
            Reference = cleanReference
        };
    }
}
