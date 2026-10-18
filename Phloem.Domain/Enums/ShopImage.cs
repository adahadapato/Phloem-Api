namespace Phloem.Domain.Enums;

/// <summary>
/// Which of a shop's two pictures a command changes or removes. Names match the Vendor's properties:
/// <see cref="Logo"/> → <c>Vendor.Logo</c>, <see cref="CoverPhoto"/> → <c>Vendor.CoverPhoto</c>.
/// </summary>
public enum ShopImage
{
    /// <summary>Round picture on shop cards (<c>Vendor.Logo</c>).</summary>
    Logo = 1,

    /// <summary>Banner on the shop page (<c>Vendor.CoverPhoto</c>).</summary>
    CoverPhoto = 2
}