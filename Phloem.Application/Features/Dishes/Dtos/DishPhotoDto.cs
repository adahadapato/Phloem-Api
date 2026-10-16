using Phloem.Domain.Entities;
using Phloem.Domain.Interfaces;

namespace Phloem.Application.Features.Dishes.Dtos;

public sealed record DishPhotoDto(
    Guid Id,
    string Url,
    string? ThumbnailUrl,
    string AltText,
    int DisplayOrder,
    bool IsPrimary)
{
    public static DishPhotoDto From(DishPhoto p, IFileStorage storage) => new(
        p.Id,
        storage.GetPublicUrl(p.File.Path),
        p.File.ThumbnailPath is null ? null : storage.GetPublicUrl(p.File.ThumbnailPath),
        p.AltText,
        p.DisplayOrder,
        p.IsPrimary);
}