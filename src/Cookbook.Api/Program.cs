using System.Text.Json.Serialization;
using Cookbook.Api.Endpoints;
using Cookbook.Infrastructure;
using Cookbook.Infrastructure.Persistence;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Http.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.FileProviders;
using Serilog;
using Serilog.Events;
using Serilog.Sinks.Grafana.Loki;

var builder = WebApplication.CreateBuilder(args);

builder.Host.UseSerilog((context, services, logger) =>
{
    logger.ReadFrom.Configuration(context.Configuration)
        .ReadFrom.Services(services)
        .Enrich.FromLogContext()
        .MinimumLevel.Information()
        .MinimumLevel.Override("Microsoft.AspNetCore", LogEventLevel.Warning)
        .MinimumLevel.Override("Microsoft.EntityFrameworkCore", LogEventLevel.Warning)
        .WriteTo.Console();

    var lokiUri = context.Configuration["Logging:Loki:Uri"];
    if (Uri.TryCreate(lokiUri, UriKind.Absolute, out var uri)
        && uri.Scheme is "http" or "https")
    {
        logger.WriteTo.GrafanaLoki(uri.ToString(),
        [
            new LokiLabel { Key = "service_name", Value = "cookbook-api" },
            new LokiLabel { Key = "environment", Value = context.HostingEnvironment.EnvironmentName.ToLowerInvariant() }
        ]);
    }
});

builder.Services.AddProblemDetails();
builder.Services.AddOpenApi();
builder.Services.AddInfrastructure(builder.Configuration);
builder.Services.Configure<JsonOptions>(o => o.SerializerOptions.Converters.Add(new JsonStringEnumConverter()));
builder.Services.Configure<FormOptions>(o => o.MultipartBodyLengthLimit = 300L * 1024 * 1024);
builder.WebHost.ConfigureKestrel(k => k.Limits.MaxRequestBodySize = 300L * 1024 * 1024);

var storage = new MediaStorage(builder.Configuration["Storage:MediaPath"] ?? "App_Data/media");
builder.Services.AddSingleton(storage);
builder.Services.AddSingleton<RemoteImageDownloader>();

builder.Services.AddCors(o => o.AddDefaultPolicy(p => p
    .WithOrigins("http://localhost:4200", "http://localhost:5000", "https://localhost:5001").AllowAnyHeader().AllowAnyMethod()));

var app = builder.Build();
app.UseSerilogRequestLogging();

// Adopt databases created by the previous EnsureCreated-based startup, then apply migrations.
using (var scope = app.Services.CreateScope())
{
    var connection = builder.Configuration.GetConnectionString("Default");
    if (connection?.StartsWith("Data Source=", StringComparison.OrdinalIgnoreCase) == true
        && Path.GetDirectoryName(connection["Data Source=".Length..]) is { Length: > 0 } dir)
    {
        Directory.CreateDirectory(dir);
    }
    var db = scope.ServiceProvider.GetRequiredService<CookbookDbContext>();
    AdoptEnsureCreatedDatabase(db);
    db.Database.Migrate();
}

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapGet("/swagger", () => Results.Content(ApiDocs.SwaggerUi, "text/html; charset=utf-8"))
        .ExcludeFromDescription();
    app.MapGet("/redoc", () => Results.Content(ApiDocs.Redoc, "text/html; charset=utf-8"))
        .ExcludeFromDescription();
}

app.UseExceptionHandler();
app.UseCors();
app.UseDefaultFiles();
app.UseStaticFiles();
app.UseStaticFiles(new StaticFileOptions
{
    FileProvider = new PhysicalFileProvider(storage.Root),
    RequestPath = "/media"
});

app.MapGet("/api", (IHostEnvironment environment) => Results.Ok(new
{
    name = "Cookbook.Api",
    environment = environment.EnvironmentName,
    status = "ok"
})).ExcludeFromDescription();

app.MapGet("/health", () => Results.Ok(new
{
    status = "healthy",
    timestampUtc = DateTime.UtcNow
}));

app.MapRecipesEndpoints();

// Frontend Angular (wwwroot) con fallback SPA, escluse API, media, health e documentazione.
if (app.Environment.WebRootPath is { } webRoot && File.Exists(Path.Combine(webRoot, "index.html")))
{
    app.MapFallbackToFile("{*path:regex(^(?!api(/|$)|media/|health$|swagger|redoc|openapi/).*$)}", "index.html");
}

app.Run();

static void AdoptEnsureCreatedDatabase(CookbookDbContext db)
{
    var connection = db.Database.GetDbConnection();
    connection.Open();
    try
    {
        var tables = connection.GetSchema("Tables");
        var hasRecipes = tables.Rows.Cast<System.Data.DataRow>()
            .Any(row => string.Equals(row["TABLE_NAME"]?.ToString(), "Recipes", StringComparison.OrdinalIgnoreCase));
        var hasHistory = tables.Rows.Cast<System.Data.DataRow>()
            .Any(row => string.Equals(row["TABLE_NAME"]?.ToString(), "__EFMigrationsHistory", StringComparison.OrdinalIgnoreCase));
        if (!hasRecipes || hasHistory) return;

        db.Database.ExecuteSqlRaw("""
            CREATE TABLE "__EFMigrationsHistory" (
                "MigrationId" character varying(150) NOT NULL CONSTRAINT "PK___EFMigrationsHistory" PRIMARY KEY,
                "ProductVersion" character varying(32) NOT NULL
            );
            INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
            VALUES ('20261121165600_Initial', '10.0.15');
            """);
    }
    finally
    {
        connection.Close();
    }
}

public partial class Program;
