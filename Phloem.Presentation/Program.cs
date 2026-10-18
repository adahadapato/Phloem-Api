using Phloem.Persistence.Seeding;
using Phloem.Presentation;
using Phloem.Presentation.Extensions;
using Phloem.Presentation.Settings.Authentication;
using Phloem.Presentation.Settings.OpenApi;
using Phloem.Worker;
using Serilog;

// Until settings are read, log start-up problems to the console
Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .CreateBootstrapLogger();

try
{
    var builder = WebApplication.CreateBuilder(args);

    // ---------- Logging: Serilog, configured from the "Serilog" section ----------
    builder.Services.AddSerilog((services, logger) => logger
        .ReadFrom.Configuration(builder.Configuration)
        .ReadFrom.Services(services));

    // ---------- Layers (each registers only its own services) ----------
    builder.Services
         .AddPresentation(builder.Configuration)      // layers, OpenAPI, authentication, errors, CurrentUser
         .AddWorker(builder.Configuration);           // scheduled jobs (later)

    var app = builder.Build();

    // ---------- Database: migrations, roles, first admin ----------
    await app.Services.SeedDatabaseAsync();

    // ---------- Pipeline ----------
    app.UseExceptionHandler();       // first, so it catches errors from everything after it
    app.UseSerilogRequestLogging();  // one line per request: method, path, status, time
    app.UseHttpsRedirection();
    app.UsePhloemOpenApi();          // /openapi/v1.json + /swagger (when OpenApi:Enabled)
    app.UsePhloemAuthentication();   // who is calling (token), then are they allowed

    // ==================== Endpoint Registration ====================
    app.MapPhloemEndpoints();        // /api/* and /health


    app.Run();
}
catch (Exception ex) when (ex is not HostAbortedException)
{
    // A start-up crash (missing setting, database unreachable…) reaches the log file instead of vanishing
    Log.Fatal(ex, "Phloem API failed to start.");
    throw;
}
finally
{
    Log.CloseAndFlush();
}