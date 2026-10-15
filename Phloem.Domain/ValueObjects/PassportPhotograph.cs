using Phloem.Domain.Shared;

namespace Phloem.Domain.ValueObjects;



/// <summary>
/// Represents a passport-sized photograph stored directly in the database.
/// This is a value object — it has no identity and is defined by its content and metadata.
/// </summary>
public class PassportPhotograph : ValueObject
{
    /// <summary>
    /// Gets the raw image data (e.g., JPG, PNG bytes).
    /// </summary>
    public byte[] Content { get; private set; }

    /// <summary>
    /// Gets the MIME type (e.g., image/jpeg, image/png).
    /// </summary>
    public string MimeType { get; private set; }

    /// <summary>
    /// Gets the upload timestamp.
    /// </summary>
    public DateTime UploadedAt { get; private set; }

    // EF Core constructor
    private PassportPhotograph() { }

    private PassportPhotograph(byte[] content, string mimeType, DateTime uploadedAt)
    {
        if (content == null || content.Length == 0)
            throw new ArgumentException("Image content is required.", nameof(content));

        if (content.Length > 1_000_000) // 1MB
            throw new ArgumentException("Image size must not exceed 1MB.", nameof(content));

        if (string.IsNullOrWhiteSpace(mimeType) || !mimeType.StartsWith("image/"))
            throw new ArgumentException("Valid image MIME type is required.", nameof(mimeType));

        Content = content;
        MimeType = mimeType;
        UploadedAt = uploadedAt;
    }

    /// <summary>
    /// Creates a <see cref="PassportPhotograph"/> from uploaded file data.
    /// </summary>
    public static Result<PassportPhotograph> Create(byte[] fileBytes, string fileName, DateTime? uploadedAt = null)
    {
        if (fileBytes == null || fileBytes.Length == 0)
            return Result<PassportPhotograph>.Failure(new Error("PassportPhotograph.Empty", "Image data is empty."));

        if (fileBytes.Length > 1_000_000)
            return Result<PassportPhotograph>.Failure(new Error("PassportPhotograph.TooLarge", "Image exceeds 1MB."));

        if (string.IsNullOrWhiteSpace(fileName))
            return Result<PassportPhotograph>.Failure(new Error("PassportPhotograph.InvalidName", "File name is required."));

        var mimeType = GetMimeTypeFromFileName(fileName);
        if (mimeType == null)
            return Result<PassportPhotograph>.Failure(new Error("PassportPhotograph.UnsupportedFormat", "Only JPG, PNG, and GIF are supported."));

        return Result<PassportPhotograph>.Success(
            new PassportPhotograph(
                content: fileBytes,
                mimeType: mimeType,
                uploadedAt: uploadedAt ?? DateTime.UtcNow));
    }

    private static string? GetMimeTypeFromFileName(string fileName)
    {
        var ext = Path.GetExtension(fileName).ToLowerInvariant();
        return ext switch
        {
            ".jpg" or ".jpeg" => "image/jpeg",
            ".png" => "image/png",
            ".gif" => "image/gif",
            _ => null
        };
    }

    protected override IEnumerable<object> GetAtomicValues()
    {
        yield return Content; // Compare by content
        yield return MimeType;
        yield return UploadedAt;
    }
}