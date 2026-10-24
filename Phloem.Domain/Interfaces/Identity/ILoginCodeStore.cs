namespace Phloem.Domain.Interfaces.Identity;

/// <summary>
/// One-time codes that carry an outside sign-in (Google) from the browser back into the app.
/// </summary>
/// <remarks>
/// <para><b>Why a code:</b> after the provider's page, the API sends the app to <c>phloem://auth?code=…</c>. Tokens
/// never travel in that link (links end up in browser history and logs); the app swaps the code for tokens with a
/// normal POST.</para>
/// <para><b>Rules:</b> a code works ONCE, and only for <c>Authentication:LoginCodeSeconds</c> (e.g. 60 seconds).
/// It only remembers WHICH account was vouched for; the session is started when the code is swapped, so every
/// sign-in check (suspended, 2FA…) runs at that moment.</para>
/// <para>Kept in the server's memory: fine with one copy of the API (today's hosting). Running several copies would
/// need a shared store.</para>
/// </remarks>
public interface ILoginCodeStore
{
    /// <summary>Makes a new one-time code for an account the provider has just vouched for.</summary>
    /// <param name="userId">The account.</param>
    /// <returns>The code to put in the link back to the app (random, link-safe).</returns>
    string Issue(Guid userId);

    /// <summary>Uses a code up. A second attempt with the same code fails.</summary>
    /// <param name="code">The code the app sent.</param>
    /// <param name="userId">The account it was made for.</param>
    /// <returns>False when the code is unknown, already used or expired.</returns>
    bool TryRedeem(string code, out Guid userId);
}
