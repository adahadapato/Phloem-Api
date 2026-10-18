using MediatR;
using Phloem.Domain.Interfaces;
using Phloem.Domain.Shared.Errors;
using Phloem.Domain.Shared.Results;
using Phloem.Domain.ValueObjects;

namespace Phloem.Application.Features.Vendors.Commands;

/// <summary>Changes the signed-in seller's shop details (the name must stay unique).</summary>
public sealed class UpdateShopDetailsCommandHandler(
    ICurrentUser currentUser,
    IVendorRepository vendors,
    IAccountService accounts,
    IUnitOfWork unitOfWork)
    : IRequestHandler<UpdateShopDetailsCommand, Result>
{
    /// <exception cref="InvalidOperationException">Called without a signed-in user (endpoint misconfigured).</exception>
    public async Task<Result> Handle(UpdateShopDetailsCommand command, CancellationToken ct)
    {
        var userId = currentUser.UserId
            ?? throw new InvalidOperationException("This action requires a signed-in user.");

        var vendor = await vendors.GetByOwnerIdAsync(userId, ct);
        if (vendor is null) return VendorErrors.NotFound;

        if (await vendors.NameExistsAsync(command.ShopName.Trim(), exceptVendorId: vendor.Id, ct))
            return VendorErrors.NameTaken;

        // Blank email / phone → the login account's (same rule as opening the shop)
        var email = command.BusinessEmail;
        var phone = command.BusinessPhone;
        if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(phone))
        {
            var contact = await accounts.GetContactAsync(userId, ct);
            if (contact.IsFailure) return contact.Error;

            if (string.IsNullOrWhiteSpace(email)) email = contact.Value.Email;
            if (string.IsNullOrWhiteSpace(phone)) phone = contact.Value.PhoneNumber;
        }

        var updated = vendor.UpdateDetails(command.ShopName, EmailAddress.Create(email),
                                           PhoneNumber.CreateOptional(phone));
        if (updated.IsFailure) return updated;

        await unitOfWork.SaveChangesAsync(ct);
        return Result.Success();
    }
}