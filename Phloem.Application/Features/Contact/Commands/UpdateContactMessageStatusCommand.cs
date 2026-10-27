using MediatR;
using Phloem.Domain.Enums;
using Phloem.Domain.Interfaces;
using Phloem.Domain.Shared.Errors;
using Phloem.Domain.Shared.Results;

namespace Phloem.Application.Features.Contact.Commands;

/// <summary>Admin → Contact messages: mark a message Replied, Closed or back to New, with an optional note.</summary>
/// <param name="Id">The message.</param>
/// <param name="Status">The new status.</param>
/// <param name="Note">A note for colleagues (empty removes it).</param>
public sealed record UpdateContactMessageStatusCommand(Guid Id, ContactMessageStatus Status, string? Note)
    : IRequest<Result>;

/// <summary>Handles <see cref="UpdateContactMessageStatusCommand"/>.</summary>
public sealed class UpdateContactMessageStatusCommandHandler(
    IContactMessageRepository messages,
    IUnitOfWork unitOfWork,
    ICurrentUser currentUser,
    TimeProvider clock)
    : IRequestHandler<UpdateContactMessageStatusCommand, Result>
{
    /// <exception cref="InvalidOperationException">Called without a signed-in user (endpoint misconfigured).</exception>
    public async Task<Result> Handle(UpdateContactMessageStatusCommand command, CancellationToken ct)
    {
        var staffId = currentUser.UserId
            ?? throw new InvalidOperationException("This action requires a signed-in user.");

        var message = await messages.GetByIdAsync(command.Id, ct);
        if (message is null) return ContactMessageErrors.NotFound;

        message.SetStatus(command.Status, command.Note, staffId, clock.GetUtcNow().UtcDateTime);
        await unitOfWork.SaveChangesAsync(ct);
        return Result.Success();
    }
}
