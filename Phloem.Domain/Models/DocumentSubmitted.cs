using Phloem.Domain.Entities;
using Phloem.Domain.ValueObjects;

namespace Phloem.Domain.Models;

/// <summary>
/// What Vendor.SubmitDocument gives back: the new document, plus the file it replaced (if any)
/// so the Application layer can delete that old file from storage after saving.
/// </summary>
public sealed record DocumentSubmitted(VendorDocument Document, DocumentFile? ReplacedFile);