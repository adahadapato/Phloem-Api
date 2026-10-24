using System.Collections.Concurrent;
using System.Security.Cryptography;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Options;
using Phloem.Domain.Interfaces.Identity;

namespace Phloem.Infrastructure.Settings.Identity;

/// <summary>
/// <see cref="ILoginCodeStore"/> kept in the server's memory: each code is random, works once, and expires after
/// <see cref="LoginCodeSettings.LoginCodeSeconds"/>.
/// </summary>
/// <remarks>
/// <para><b>Once only:</b> redeeming REMOVES the code in one atomic step, so two requests with the same code can't
/// both succeed.</para>
/// <para><b>Tidy:</b> expired codes are cleared whenever a new code is issued, so the list never grows.</para>
/// <para><b>One copy of the API:</b> codes live in this process. A restart (or a second copy behind a load balancer)
/// loses them; the person just taps "Continue with Google" again.</para>
/// <para>Registered as a singleton in <see cref="IdentitySetup"/>.</para>
/// </remarks>
internal sealed class MemoryLoginCodeStore(IOptions<LoginCodeSettings> options, TimeProvider clock) : ILoginCodeStore
{
    /// <summary>Random bytes per code: 256 bits, impossible to guess.</summary>
    private const int CodeBytes = 32;

    /// <summary>Waiting codes: code → (account, expiry time in UTC).</summary>
    private readonly ConcurrentDictionary<string, (Guid UserId, DateTime ExpiresAtUtc)> codes =
        new(StringComparer.Ordinal);

    /// <inheritdoc />
    public string Issue(Guid userId)
    {
        var now = Now;
        RemoveExpired(now);

        var code = WebEncoders.Base64UrlEncode(RandomNumberGenerator.GetBytes(CodeBytes));
        codes[code] = (userId, now.AddSeconds(options.Value.LoginCodeSeconds));

        return code;
    }

    /// <inheritdoc />
    public bool TryRedeem(string code, out Guid userId)
    {
        userId = Guid.Empty;
        if (string.IsNullOrWhiteSpace(code)) return false;

        // Removed whatever happens next: a code is never usable twice
        if (!codes.TryRemove(code.Trim(), out var entry)) return false;
        if (entry.ExpiresAtUtc <= Now) return false;

        userId = entry.UserId;
        return true;
    }

    /// <summary>The current time (UTC), from the app's clock.</summary>
    private DateTime Now => clock.GetUtcNow().UtcDateTime;

    /// <summary>Forgets every code whose time is up.</summary>
    /// <param name="now">The current time (UTC).</param>
    private void RemoveExpired(DateTime now)
    {
        foreach (var (code, entry) in codes)
            if (entry.ExpiresAtUtc <= now)
                codes.TryRemove(code, out _);
    }
}
