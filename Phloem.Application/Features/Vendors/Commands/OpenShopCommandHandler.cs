using Phloem.Domain.Enums;
using Phloem.Application.Common.Terms;
using MediatR;
using Phloem.Domain.Entities;
using Phloem.Domain.Interfaces;
using Phloem.Domain.Shared.Errors;
using Phloem.Domain.Shared.Results;
using Phloem.Domain.ValueObjects;

namespace Phloem.Application.Features.Vendors.Commands;

/// <summary>
/// The signed-in user opens their shop. Buyers can't see it until an admin approves it,
/// so no cache needs clearing here.
/// </summary>
/// <remarks>The owner must have their date of birth and home address on their profile, and be 18 or older
/// (<see cref="UserProfile.CheckCanTrade"/>).</remarks>
public sealed class OpenShopCommandHandler(
    ICurrentUser currentUser,
    IUserProfileRepository profiles,
    IVendorRepository vendors,
    IAccountService accounts,
    IUnitOfWork unitOfWork,
    TimeProvider time,
    TermsGate terms)
    : IRequestHandler<OpenShopCommand, Result<Guid>>
{
    public async Task<Result<Guid>> Handle(OpenShopCommand command, CancellationToken ct)
    {
        var userId = currentUser.UserId
            ?? throw new InvalidOperationException("This action requires a signed-in user.");

        // ---------- Checks that need the database ----------
        var profile = await profiles.GetByIdAsync(userId, ct);
        if (profile is null) return UserProfileErrors.NotFound;

        // NEW: date of birth + home address filled in, and 18 or older
        var canTrade = profile.CheckCanTrade(DateOnly.FromDateTime(time.GetUtcNow().UtcDateTime));
        if (canTrade.IsFailure) return canTrade.Error;

        if (await vendors.GetByOwnerIdAsync(userId, ct) is not null)
            return VendorErrors.AlreadyHasShop;

        if (await vendors.NameExistsAsync(command.ShopName.Trim(), exceptVendorId: null, ct))
            return VendorErrors.NameTaken;

        // ---------- Blank email / phone → use the login account's ----------
        var email = command.BusinessEmail;
        var phone = command.BusinessPhone;

        if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(phone))
        {
            var contact = await accounts.GetContactAsync(userId, ct);
            if (contact.IsFailure) return contact.Error;

            if (string.IsNullOrWhiteSpace(email)) email = contact.Value.Email;
            if (string.IsNullOrWhiteSpace(phone)) phone = contact.Value.PhoneNumber;
        }

        // ---------- Business rules (Domain) ----------
        var opened = Vendor.Open(profile, command.ShopName,
                                 EmailAddress.Create(email), PhoneNumber.CreateOptional(phone));
        if (opened.IsFailure) return opened.Error;

        vendors.Add(opened.Value);

        // Proof of "I agree to the Seller terms" (the Terms version published now), saved with the shop
        await terms.RecordAsync(userId, LegalDocumentType.Terms, AcceptanceContext.OpenShop, null, ct);

        await unitOfWork.SaveChangesAsync(ct);

        return opened.Value.Id;
    }
}