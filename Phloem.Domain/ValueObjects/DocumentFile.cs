namespace Phloem.Domain.ValueObjects;

/// <summary>
/// A document stored in the PRIVATE container exactly as uploaded (never resized or compressed:
/// it's evidence). Only admins and the shop owner can open it, through a short-lived link.
/// </summary>
public sealed class DocumentFile : ValueObject
{
    public const long MaxFileSizeBytes = 10 * 1024 * 1024;   // 10 MB

    public static readonly IReadOnlyList<string> AllowedContentTypes =
        ["application/pdf", "image/jpeg", "image/png"];

    public const string InvalidFormatMessage = "Please upload a PDF, JPEG or PNG file.";
    public const string TooLargeMessage = "The file is too large. The maximum is 10 MB.";

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

    public string ContentType { get; }
    public long FileSizeBytes { get; }

    /// <summary>SHA-256 of the file (worked out by Infrastructure).</summary>
    public string Hash { get; }

    public static DocumentFile Create(string path, string originalFileName, string contentType,
                                      long fileSizeBytes, string hash)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(path);
        ArgumentException.ThrowIfNullOrWhiteSpace(originalFileName);
        ArgumentException.ThrowIfNullOrWhiteSpace(contentType);
        ArgumentException.ThrowIfNullOrWhiteSpace(hash);
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(fileSizeBytes);

        var type = contentType.Trim().ToLowerInvariant();
        if (!AllowedContentTypes.Contains(type))
            throw new ArgumentException(InvalidFormatMessage, nameof(contentType));
        if (fileSizeBytes > MaxFileSizeBytes)
            throw new ArgumentException(TooLargeMessage, nameof(fileSizeBytes));

        return new DocumentFile(path.Trim(), originalFileName.Trim(), type, fileSizeBytes, hash.Trim().ToUpperInvariant());
    }

    protected override IEnumerable<object?> GetAtomicValues()
    {
        yield return Path;
        yield return OriginalFileName;
        yield return ContentType;
        yield return FileSizeBytes;
        yield return Hash;
    }
}