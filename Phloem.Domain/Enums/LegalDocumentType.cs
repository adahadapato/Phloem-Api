namespace Phloem.Domain.Enums;

/// <summary>The legal texts people agree to or read. Shown by the website and the app from the database.</summary>
/// <remarks>The numbers are stored in the database: only add new values at the end.</remarks>
public enum LegalDocumentType
{
    /// <summary>Terms &amp; Conditions (buyers, sellers and parcels, in sections).</summary>
    Terms = 1,

    /// <summary>Privacy Policy: what personal data Phloem keeps, why, and people's rights.</summary>
    Privacy = 2
}
