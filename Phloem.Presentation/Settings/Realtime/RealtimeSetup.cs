using System.Text.Json.Serialization;
using Microsoft.AspNetCore.SignalR;
using Phloem.Application.Features.Chat.Common;
using Phloem.Presentation.Realtime;

namespace Phloem.Presentation.Settings.Realtime;

/// <summary>
/// Live updates (SignalR, built into ASP.NET Core): the order chat at <c>/hubs/chat</c>.
/// The page signs in with the same access token as the API, sent as <c>?access_token=</c> because browsers can't add
/// headers to WebSockets (read in <c>AuthenticationSetup</c>, only for <c>/hubs</c>).
/// </summary>
/// <remarks>One API server needs nothing else. With several servers, add Azure SignalR Service (one line here) so a
/// message reaches people connected to another server.</remarks>
public static class RealtimeSetup
{
    /// <summary>Where pages connect.</summary>
    public const string ChatHubPath = "/hubs/chat";

    /// <summary>Registers SignalR (enums as names, like the API) and the chat's live sender.</summary>
    /// <param name="services">The service collection.</param>
    /// <returns>The same service collection, for chaining.</returns>
    public static IServiceCollection AddPhloemRealtime(this IServiceCollection services)
    {
        services.AddSignalR(options => options.MaximumReceiveMessageSize = 4 * 1024)   // pages only send "typing"
                .AddJsonProtocol(options =>
                        options.PayloadSerializerOptions.Converters.Add(new JsonStringEnumConverter()));
        services.AddSingleton<IUserIdProvider, ChatUserIdProvider>();
        services.AddSingleton<IChatRealtime, ChatRealtime>();
        return services;
    }

    /// <summary>Maps the hubs (signed-in people only; not in Swagger).</summary>
    /// <param name="app">The web application.</param>
    /// <returns>The same application, for chaining.</returns>
    public static WebApplication MapPhloemHubs(this WebApplication app)
    {
        app.MapHub<ChatHub>(ChatHubPath).RequireAuthorization();
        return app;
    }
}
