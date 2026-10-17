using Phloem.Persistence.Seeding;
using Phloem.Presentation;
using Phloem.Presentation.Settings.OpenApi;
using Phloem.Worker;

var builder = WebApplication.CreateBuilder(args);

// TEMPORARY: don't check every handler's services at start-up.
// ICurrentUser, IAuthService, IAccountService and IUserRoleService aren't implemented yet; remove this block
// once they are, so a missing registration is caught at start-up again instead of on the first request.
builder.Host.UseDefaultServiceProvider(options =>
{
    options.ValidateScopes = builder.Environment.IsDevelopment();   // still catch scoped-in-singleton mistakes
    options.ValidateOnBuild = false;
});

// ---------- Layers (each registers only its own services) ----------
builder.Services
     .AddPresentation(builder.Configuration)      // layers, OpenAPI; endpoints, auth, CurrentUser (to come)
     .AddWorker(builder.Configuration);           // scheduled jobs (later)

var app = builder.Build();

// ---------- Database: migrations, roles, first admin ----------
await app.Services.SeedDatabaseAsync();

// ---------- Pipeline ----------
app.UseHttpsRedirection();
app.UsePhloemOpenApi();          // /openapi/v1.json + /swagger (when OpenApi:Enabled)

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