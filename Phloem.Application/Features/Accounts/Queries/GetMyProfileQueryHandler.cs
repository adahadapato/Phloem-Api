using Phloem.Application.Features.LegalDocuments.Dtos;
using Phloem.Application.Common.Terms;
using MediatR;
using Phloem.Application.Features.Accounts.Dtos;
using Phloem.Domain.Interfaces;
using Phloem.Domain.Shared.Errors;
using Phloem.Domain.Shared.Results;

namespace Phloem.Application.Features.Accounts.Queries;

public sealed class GetMyProfileQueryHandler(
    ICurrentUser currentUser,
    IUserProfileRepository profiles,
    IVendorRepository vendors,
    IAccountService accounts,
    IFileStorage storage,
    TermsGate terms)
    : IRequestHandler<GetMyProfileQuery, Result<ProfileDto>>
{
    public async Task<Result<ProfileDto>> Handle(GetMyProfileQuery query, CancellationToken ct)
    {
        var userId = currentUser.UserId
            ?? throw new InvalidOperationException("This action requires a signed-in user.");

        var profile = await profiles.GetByIdAsync(userId, ct);
        if (profile is null) return UserProfileErrors.NotFound;

        var contact = await accounts.GetContactAsync(userId, ct);
        if (contact.IsFailure) return contact.Error;

        // Account page: "Seller dashboard" if they have a shop (even while waiting for approval)
        var hasShop = await vendors.GetByOwnerIdAsync(userId, ct) is not null;

        // DB stores the path; the app needs the full link
        var photoUrl = profile.PhotoUrl is null ? null : storage.GetPublicUrl(profile.PhotoUrl);

        // "We've updated our Terms" / Privacy notice on the website and app
        var status = await terms.GetStatusAsync(userId, ct);

        return ProfileDto.From(profile, contact.Value, photoUrl, hasShop) with { Terms = TermsStatusDto.From(status) };
    }
}