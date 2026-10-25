using Phloem.Domain.Enums;
using Phloem.Domain.ValueObjects;

namespace Phloem.Domain.Entities;

/// <summary>
/// A saved delivery address. Belongs to a UserProfile and is only changed through it
/// (so the "exactly one default" rule can't be broken from outside).
/// </summary>
/// <remarks>
/// <see cref="RecipientName"/> and <see cref="PhoneNumber"/> say who receives the food and who the seller calls
/// (e.g. "Mum" at her number). Both are optional: addresses saved before they existed have neither, and the
/// account holder's own name and phone are used instead.
/// </remarks>
public class DeliveryAddress : Entity<Guid>
{
    // ---------- Limits (used by validators in Application and column sizes in Persistence) ----------
    public const int LabelMaxLength = 40;          // "Home", "Work", "Mum's house"
    public const int FullAddressMaxLength = 300;
    public const int RecipientNameMaxLength = 100;  // "Ama Mensah"

    protected DeliveryAddress() { }

    internal DeliveryAddress(Guid userId, string label, string fullAddress, Country country, string? recipientName,
                             PhoneNumber? phoneNumber)
        : base(Guid.CreateVersion7())
    {
        UserId = userId;
        Set(label, fullAddress, country, recipientName, phoneNumber);
    }

    public Guid UserId { get; private set; }
    public string Label { get; private set; } = string.Empty;
    public string FullAddress { get; private set; } = string.Empty;
    public Country Country { get; private set; }
    public bool IsDefault { get; private set; }

    /// <summary>Who receives the food, e.g. "Ama Mensah"; null = the account holder.</summary>
    public string? RecipientName { get; private set; }

    /// <summary>Who the seller calls about the delivery; null = the account holder's phone.</summary>
    public PhoneNumber? PhoneNumber { get; private set; }

    /// <summary>Internal: change addresses through UserProfile.UpdateAddress, so its rules always apply.</summary>
    internal void Update(string label, string fullAddress, Country country, string? recipientName,
                         PhoneNumber? phoneNumber) =>
        Set(label, fullAddress, country, recipientName, phoneNumber);

    internal void MarkDefault() => IsDefault = true;
    internal void ClearDefault() => IsDefault = false;

    private void Set(string label, string fullAddress, Country country, string? recipientName, PhoneNumber? phoneNumber)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(label);
        ArgumentException.ThrowIfNullOrWhiteSpace(fullAddress);
        ArgumentOutOfRangeException.ThrowIfGreaterThan(label.Trim().Length, LabelMaxLength, nameof(label));
        ArgumentOutOfRangeException.ThrowIfGreaterThan(fullAddress.Trim().Length, FullAddressMaxLength, nameof(fullAddress));

        var name = string.IsNullOrWhiteSpace(recipientName) ? null : recipientName.Trim();
        if (name is not null)
            ArgumentOutOfRangeException.ThrowIfGreaterThan(name.Length, RecipientNameMaxLength, nameof(recipientName));

        Label = label.Trim();
        FullAddress = fullAddress.Trim();
        Country = country;
        RecipientName = name;
        PhoneNumber = phoneNumber;
    }
}