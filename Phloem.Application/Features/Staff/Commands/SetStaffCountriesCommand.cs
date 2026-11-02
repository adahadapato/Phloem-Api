using MediatR;
using Phloem.Domain.Enums;
using Phloem.Domain.Shared.Results;

namespace Phloem.Application.Features.Staff.Commands;

/// <summary>
/// Admin → Staff → a person: sets which countries a staff member covers (one, several, or all).
/// </summary>
/// <param name="UserId">The staff member (must hold a job role).</param>
/// <param name="AllCountries">True = every country, including ones Phloem adds later.</param>
/// <param name="Countries">The countries when <paramref name="AllCountries"/> is false (at least one).</param>
/// <remarks>
/// <para>Needs <c>staff.manage</c>. Nobody changes their own countries.</para>
/// <para>Someone who covers only some countries can only add or remove THEIR countries for others; the person's other
/// countries are kept, and only someone covering every country can give "all countries".</para>
/// <para>The person is signed out everywhere so the change applies at once; it's written to the audit log.</para>
/// </remarks>
public sealed record SetStaffCountriesCommand(Guid UserId, bool AllCountries, IReadOnlyList<Country>? Countries)
    : IRequest<Result>;
