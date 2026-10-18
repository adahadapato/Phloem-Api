using Phloem.Persistence.Seeding;
using Phloem.Presentation;
using Phloem.Presentation.Settings.Authentication;
using Phloem.Presentation.Settings.OpenApi;
using Phloem.Worker;

var builder = WebApplication.CreateBuilder(args);

// ---------- Layers (each registers only its own services) ----------
builder.Services
     .AddPresentation(builder.Configuration)      // layers, OpenAPI, authentication, CurrentUser
     .AddWorker(builder.Configuration);           // scheduled jobs (later)

var app = builder.Build();

// ---------- Database: migrations, roles, first admin ----------
await app.Services.SeedDatabaseAsync();

// ---------- Pipeline ----------
app.UseHttpsRedirection();
app.UsePhloemOpenApi();          // /openapi/v1.json + /swagger (when OpenApi:Enabled)
app.UsePhloemAuthentication();   // who is calling (token), then are they allowed

// ==================== Endpoint Registration ====================
var api = app.MapGroup("/api");

app.MapGet("/health",
        () => Results.Ok(
            new
            {
                status = "healthy",
                timestamp = DateTime.UtcNow
            }))
    .ExcludeFromDescription();

app.Run();