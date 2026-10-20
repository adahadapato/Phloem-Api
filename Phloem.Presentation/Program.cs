
using Phloem.Persistence.Seeding;
using Phloem.Presentation;
using Phloem.Presentation.Extensions;
using Phloem.Presentation.Settings.Authentication;
using Phloem.Presentation.Settings.OpenApi;
using Phloem.Worker;
using Serilog;

var diagnosticDirectory = Path.Combine(AppContext.BaseDirectory, "App_Data");

var diagnosticFile = Path.Combine(
    diagnosticDirectory,  "startup-diagnostics.log");

void WriteStartupDiagnostic(string message, Exception? exception = null)
{
    try
    {
        Directory.CreateDirectory(diagnosticDirectory);

        var entry =
            $"[{DateTimeOffset.UtcNow:yyyy-MM-dd HH:mm:ss.fff zzz}] {message}" +
            Environment.NewLine;

        if (exception is not null)
        {
            entry += exception + Environment.NewLine;
        }

        File.AppendAllText(diagnosticFile, entry);
    }
    catch
    {
        // Diagnostic logging must never prevent the API from starting.
    }
}

// Bootstrap logger for failures before application logging is configured.
Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .CreateBootstrapLogger();

try
{
    WriteStartupDiagnostic("Phloem API startup initiated.");

    Log.Information("Creating Phloem application builder.");

    var builder = WebApplication.CreateBuilder(args);

    WriteStartupDiagnostic($"Environment: {builder.Environment.EnvironmentName}");

    WriteStartupDiagnostic($"Content root: {builder.Environment.ContentRootPath}");

    // Configure Serilog from application configuration.
    builder.Services.AddSerilog((services, logger) => logger
        .ReadFrom.Configuration(builder.Configuration)
        .ReadFrom.Services(services));

    WriteStartupDiagnostic("Registering application services.");

    builder.Services
        .AddPresentation(builder.Configuration)
        .AddWorker(builder.Configuration);

    WriteStartupDiagnostic("Application services registered.");

    WriteStartupDiagnostic("Building application.");

    var app = builder.Build();

    WriteStartupDiagnostic("Application built successfully.");

    // Database initialization.
    WriteStartupDiagnostic("Starting database initialization.");

    Log.Information("Starting Phloem database initialization.");

    await app.Services.SeedDatabaseAsync();

    WriteStartupDiagnostic("Database initialization completed.");

    Log.Information("Phloem database initialization completed.");

    // HTTP pipeline.
    WriteStartupDiagnostic("Configuring HTTP middleware.");

    app.UseSerilogRequestLogging();
    app.UseExceptionHandler();
    // Development: no redirect, so the Android emulator can load pictures over http://10.0.2.2:5175
    // (it doesn't trust the local development certificate). The live site always redirects to https.
    if (!app.Environment.IsDevelopment())
        app.UseHttpsRedirection();
    app.UsePhloemStaticFiles();
    app.UsePhloemOpenApi();
    app.UsePhloemAuthentication();

    WriteStartupDiagnostic("Registering API endpoints.");

    app.MapPhloemEndpoints();

    WriteStartupDiagnostic("API endpoints registered.");

    app.Lifetime.ApplicationStarted.Register(() =>
    {
        WriteStartupDiagnostic("Phloem API started successfully.");
    });

    WriteStartupDiagnostic("Starting web host.");

    app.Run();
}
catch (Exception ex) when (ex is not HostAbortedException)
{
    WriteStartupDiagnostic("FATAL: Phloem API failed to start.",  ex);

    Log.Fatal(ex, "Phloem API failed to start.");

    throw;
}
finally
{
    Log.CloseAndFlush();
}
