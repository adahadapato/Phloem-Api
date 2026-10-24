using Phloem.Application;
using Phloem.Infrastructure;
using Phloem.Persistence;
using Phloem.Presentation.Middleware;
using Phloem.Presentation.Settings.Authentication;
using Phloem.Presentation.Settings.OpenApi;
using System;
using System.Text.Json.Serialization;
using static Azure.Core.HttpHeader;

namespace Phloem.Presentation;

/// <summary>
/// The single composition root: wires every layer in one place, so Program.cs stays a short list of steps.
/// <list type="number">
///   <item><description><b>Layers:</b> Application, Persistence, Infrastructure (Worker later).</description></item>
///   <item><description><b>API:</b> OpenAPI / Swagger (then authentication, policies, CORS, rate limiting…).</description></item>
/// </list>
/// </summary>
/// <remarks>Each layer only registers its OWN services through its own <c>Add…</c> method; only this class knows
/// about all of them.</remarks>
public static class DependencyInjection
{
    /// <summary>Adds every layer plus the API's own services.</summary>
    /// <param name="services">The service collection.</param>
    /// <param name="configuration">The app's configuration.</param>
    /// <returns>The same service collection, for chaining.</returns>
    public static IServiceCollection AddPresentation(this IServiceCollection services, IConfiguration configuration)
    {
        // 1. Layers
        services.AddApplication();                   // MediatR pipeline, validators, caching, TimeProvider
        services.AddPersistence(configuration);      // SQL Server DbContext, unit of work, repositories, seeding
        services.AddInfrastructure(configuration);   // settings, email, storage, images (Identity next)
                                                     // services.AddWorker(configuration);         // scheduled jobs (later)

        // 2. API
        services.AddPhloemOpenApi(configuration);    // 2a. OpenAPI document + Swagger page
        services.AddPhloemAuthentication(configuration);   // 2b. JWT bearer (+ Google when set up), authorization, CurrentUser

        // 2c. Errors: one ProblemDetails shape for thrown exceptions AND Result failures
        services.AddProblemDetails();
        services.AddExceptionHandler<GlobalExceptionHandler>();

        // 2d.JSON: enums as names ("Ghana", not 2), both in and out; numbers are still accepted
        services.ConfigureHttpJsonOptions(options =>
                options.SerializerOptions.Converters.Add(new JsonStringEnumConverter()));
        // 2b. authentication + JWT bearer, 2c. policies, 2d. CurrentUser, 2e. CORS, 2f. rate limiting,
        // 2g. exception handler

        return services;
    }
}