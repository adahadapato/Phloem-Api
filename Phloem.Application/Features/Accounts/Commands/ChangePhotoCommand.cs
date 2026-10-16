using MediatR;
using Phloem.Domain.Shared.Results;

namespace Phloem.Application.Features.Accounts.Commands;

/// <summary>
/// Edit profile → change photo. The endpoint passes the uploaded file's stream and size.
/// Returns the new photo's full URL so the app can show it straight away.
/// </summary>
public sealed record ChangePhotoCommand(Stream Content, long SizeBytes) : IRequest<Result<string>>;