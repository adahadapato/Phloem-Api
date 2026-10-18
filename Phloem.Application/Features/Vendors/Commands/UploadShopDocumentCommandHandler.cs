using System.Net;
using System.Security.Cryptography;
using MediatR;
using Microsoft.Extensions.Logging;
using Phloem.Domain.Interfaces;
using Phloem.Domain.Shared.Errors;
using Phloem.Domain.Shared.Results;
using Phloem.Domain.ValueObjects;

namespace Phloem.Application.Features.Vendors.Commands;

/// <summary>
/// Stores the document exactly as uploaded in the PRIVATE container, records it on the shop, then tells the team.
/// </summary>
/// <remarks>
/// <para><b>The file's real type is checked from its first bytes,</b> not from its name or what the app claims, so a
/// renamed file can't sneak in.</para>
/// <para><b>Order:</b> save file → record on shop → save DB → delete the REPLACED file (if any) → email the team.
/// If the shop refuses the document, the new file is deleted again.</para>
/// </remarks>
public sealed class UploadShopDocumentCommandHandler(
    ICurrentUser currentUser,
    IVendorRepository vendors,
    IFileStorage storage,
    IUnitOfWork unitOfWork,
    IEmailSender email,
    TimeProvider time,
    ILogger<UploadShopDocumentCommandHandler> logger)
    : IRequestHandler<UploadShopDocumentCommand, Result<Guid>>
{
    private static readonly Error InvalidFormat = Error.Validation("Vendor.DocumentFormat", DocumentFile.InvalidFormatMessage);
    private static readonly Error TooLarge = Error.Validation("Vendor.DocumentTooLarge", DocumentFile.TooLargeMessage);

    /// <exception cref="InvalidOperationException">Called without a signed-in user (endpoint misconfigured).</exception>
    public async Task<Result<Guid>> Handle(UploadShopDocumentCommand command, CancellationToken ct)
    {
        var userId = currentUser.UserId
            ?? throw new InvalidOperationException("This action requires a signed-in user.");

        var vendor = await vendors.GetByOwnerIdAsync(userId, ct);
        if (vendor is null) return VendorErrors.NotFound;

        // 1. Read it (max 10 MB) to check its real type and work out its hash
        await using var buffer = new MemoryStream();
        await command.Content.CopyToAsync(buffer, ct);
        if (buffer.Length == 0) return InvalidFormat;
        if (buffer.Length > DocumentFile.MaxFileSizeBytes) return TooLarge;

        var contentType = DetectContentType(buffer);
        if (contentType is null) return InvalidFormat;

        buffer.Position = 0;
        var hash = Convert.ToHexString(await SHA256.HashDataAsync(buffer, ct));

        // 2. Store it (private: admins and the owner only, through short-lived links)
        var extension = Extensions[contentType];
        var path = $"vendor-documents/{vendor.Id:N}/{command.Type.ToString().ToLowerInvariant()}-{Guid.CreateVersion7():N}.{extension}";
        buffer.Position = 0;
        await storage.SaveAsync(buffer, path, contentType, StorageContainer.Private, ct);

        // 3. Record it on the shop (Domain rules: blocked shop, expiry date in the past…)
        var file = DocumentFile.Create(path, CleanFileName(command.FileName, command.Type.ToString(), extension),
                                       contentType, buffer.Length, hash);
        var now = time.GetUtcNow().UtcDateTime;
        var expiresAt = command.ExpiresOn?.ToDateTime(TimeOnly.MaxValue, DateTimeKind.Utc);

        var submitted = vendor.SubmitDocument(command.Type, file, expiresAt, now);
        if (submitted.IsFailure)
        {
            await storage.DeleteAsync(path, StorageContainer.Private, ct);
            return submitted.Error;
        }

        await unitOfWork.SaveChangesAsync(ct);

        // 4. The replaced file isn't needed any more
        if (submitted.Value.ReplacedFile is { } replaced)
            await storage.DeleteAsync(replaced.Path, StorageContainer.Private, ct);

        // 5. Tell the team (never fails the upload)
        await NotifyTeamAsync(vendor.Name, command.Type.ToString(), ct);

        return submitted.Value.Document.Id;
    }

    /// <summary>File extension per allowed type.</summary>
    private static readonly Dictionary<string, string> Extensions = new()
    {
        ["application/pdf"] = "pdf",
        ["image/jpeg"] = "jpg",
        ["image/png"] = "png"
    };

    /// <summary>The real type from the first bytes ("magic numbers"); null if not PDF, JPEG or PNG.</summary>
    private static string? DetectContentType(MemoryStream file)
    {
        var bytes = file.GetBuffer().AsSpan(0, (int)Math.Min(file.Length, 8));

        if (bytes.StartsWith("%PDF"u8)) return "application/pdf";
        if (bytes.StartsWith((ReadOnlySpan<byte>)[0xFF, 0xD8, 0xFF])) return "image/jpeg";
        if (bytes.StartsWith((ReadOnlySpan<byte>)[0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A])) return "image/png";
        return null;
    }

    /// <summary>Just the name (no folders), with the right extension, short enough for the column.</summary>
    private static string CleanFileName(string? fileName, string fallback, string extension)
    {
        var name = Path.GetFileNameWithoutExtension(Path.GetFileName(fileName ?? string.Empty)).Trim();
        if (string.IsNullOrWhiteSpace(name)) name = fallback;

        var maxNameLength = DocumentFile.OriginalFileNameMaxLength - extension.Length - 1;
        if (name.Length > maxNameLength) name = name[..maxNameLength];

        return $"{name}.{extension}";
    }

    /// <summary>"New document to review" to the team inbox; a sending error is only logged.</summary>
    private async Task NotifyTeamAsync(string shopName, string documentType, CancellationToken ct)
    {
        try
        {
            await email.SendToTeamAsync(
                $"Document to review: {shopName}",
                $"<p><b>{WebUtility.HtmlEncode(shopName)}</b> uploaded a new <b>{documentType}</b> document.</p>" +
                "<p>Please review it in the admin area.</p>",
                ct);
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            logger.LogError(ex, "Couldn't email the team about {Shop}'s {DocumentType} document.", shopName, documentType);
        }
    }
}