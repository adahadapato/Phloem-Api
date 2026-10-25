namespace Phloem.Domain.Shared.Errors;

/// <summary>Every business-rule failure for legal texts. Nothing here is thrown.</summary>
public static class LegalDocumentErrors
{
    /// <summary>No version of this text has been published yet.</summary>
    public static readonly Error NotFound = Error.NotFound(
        "LegalDocument.NotFound", "This document isn't available yet.");
}
