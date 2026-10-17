namespace Phloem.Domain.ValueObjects;

/// <summary>
/// A document stored in the PRIVATE container exactly as uploaded (never resized or compressed:
/// it's evidence). Only admins and the shop owner can open it, through a short-lived link.
/// </summary>
/// <remarks>EF Core builds it through the private constructor (every parameter matches a property).</remarks>
public sealed class DocumentFile : ValueObject
{
    /// <summary>Largest upload: 10 MB.</summary>
    public const long MaxFileSizeBytes = 10 * 1024 * 1024;

    /// <summary>PDF, JPEG or PNG.</summary>
    public static readonly IReadOnlyList<string> AllowedContentTypes =
        ["application/pdf", "image/jpeg", "image/png"];

    /// <summary>Shown when the format isn't allowed.</summary>
    public const string InvalidFormatMessage = "Please upload a PDF, JPEG or PNG file.";

    /// <summary>Shown when the file is over <see cref="MaxFileSizeBytes"/>.</summary>
    public const string TooLargeMessage = "The file is too large. The maximum is 10 MB.";

    /// <summary>Longest storage path (column size).</summary>
    public const int PathMaxLength = 500;

    /// <summary>Longest original file name (column size). Infrastructure shortens longer names before calling Create.</summary>
    public const int OriginalFileNameMaxLength = 255;

    /// <summary>Longest content type (column size); the allowed ones are far shorter.</summary>
    public const int ContentTypeMaxLength = 100;

    /// <summary>Longest hash: SHA-256 written as hex is 64 characters.</summary>
    public const int HashMaxLength = 64;

    private DocumentFile(string path, string originalFileName, string contentType, long fileSizeBytes, string hash)
    {
        Path = path;
        OriginalFileName = originalFileName;
        ContentType = contentType;
        FileSizeBytes = fileSizeBytes;
        Hash = hash;
    }

    /// <summary>Where it lives in the private container, e.g. "vendor-documents/{vendorId}/{id}.pdf".</summary>
    public string Path { get; }

    /// <summary>The name the seller's file had, shown to the admin ("CAC certificate.pdf").</summary>
    public string OriginalFileName { get; }

    /// <summary>"application/pdf", "image/jpeg" or "image/png".</summary>
    public string ContentType { get; }

    /// <summary>File size in bytes.</summary>
    public long FileSizeBytes { get; }

    /// <summary>SHA-256 of the file (worked out by Infrastructure).</summary>
    public string Hash { get; }

    /// <summary>Records a stored document.</summary>
    /// <param name="path">Storage path.</param>
    /// <param name="originalFileName">The uploaded file's name.</param>
    /// <param name="contentType">One of <see cref="AllowedContentTypes"/>.</param>
    /// <param name="fileSizeBytes">1 byte to <see cref="MaxFileSizeBytes"/>.</param>
    /// <param name="hash">SHA-256 as hex.</param>
    /// <returns>The value object.</returns>
    /// <exception cref="ArgumentException">Blank text, a format that isn't allowed, or a file that's too large.</exception>
    /// <exception cref="ArgumentOutOfRangeException">Size 0 or less, or text longer than its column.</exception>
    public static DocumentFile Create(string path, string originalFileName, string contentType,
                                      long fileSizeBytes, string hash)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(path);
        ArgumentException.ThrowIfNullOrWhiteSpace(originalFileName);
        ArgumentException.ThrowIfNullOrWhiteSpace(contentType);
        ArgumentException.ThrowIfNullOrWhiteSpace(hash);
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(fileSizeBytes);
        ArgumentOutOfRangeException.ThrowIfGreaterThan(path.Trim().Length, PathMaxLength, nameof(path));
        ArgumentOutOfRangeException.ThrowIfGreaterThan(originalFileName.Trim().Length, OriginalFileNameMaxLength, nameof(originalFileName));
        ArgumentOutOfRangeException.ThrowIfGreaterThan(hash.Trim().Length, HashMaxLength, nameof(hash));

        var type = contentType.Trim().ToLowerInvariant();
        if (!AllowedContentTypes.Contains(type))
            throw new ArgumentException(InvalidFormatMessage, nameof(contentType));
        if (fileSizeBytes > MaxFileSizeBytes)
            throw new ArgumentException(TooLargeMessage, nameof(fileSizeBytes));

        return new DocumentFile(path.Trim(), originalFileName.Trim(), type, fileSizeBytes, hash.Trim().ToUpperInvariant());
    }

    /// <inheritdoc />
    protected override IEnumerable<object?> GetAtomicValues()
    {
        yield return Path;
        yield return OriginalFileName;
        yield return ContentType;
        yield return FileSizeBytes;
        yield return Hash;
    }
}