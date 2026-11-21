using System.Text.Json.Serialization;
using Cookbook.Api.Endpoints;
using Cookbook.Infrastructure;
using Cookbook.Infrastructure.Persistence;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Http.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.FileProviders;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddProblemDetails();
builder.Services.AddOpenApi();
builder.Services.AddInfrastructure(builder.Configuration);
builder.Services.Configure<JsonOptions>(o => o.SerializerOptions.Converters.Add(new JsonStringEnumConverter()));
builder.Services.Configure<FormOptions>(o => o.MultipartBodyLengthLimit = 300L * 1024 * 1024);
builder.WebHost.ConfigureKestrel(k => k.Limits.MaxRequestBodySize = 300L * 1024 * 1024);

var storage = new MediaStorage(builder.Configuration["Storage:MediaPath"] ?? "App_Data/media");
builder.Services.AddSingleton(storage);

builder.Services.AddCors(o => o.AddDefaultPolicy(p => p
    .WithOrigins("http://localhost:4200").AllowAnyHeader().AllowAnyMethod()));

var app = builder.Build();

// Nessuna migrazione: lo schema viene creato al primo avvio.
using (var scope = app.Services.CreateScope())
{
    var connection = builder.Configuration.GetConnectionString("Default");
    if (connection?.StartsWith("Data Source=", StringComparison.OrdinalIgnoreCase) == true
        && Path.GetDirectoryName(connection["Data Source=".Length..]) is { Length: > 0 } dir)
    {
        Directory.CreateDirectory(dir);
    }
    scope.ServiceProvider.GetRequiredService<CookbookDbContext>().Database.EnsureCreated();
}

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseExceptionHandler();
app.UseCors();
app.UseStaticFiles(new StaticFileOptions
{
    FileProvider = new PhysicalFileProvider(storage.Root),
    RequestPath = "/media"
});

app.MapGet("/", (IHostEnvironment environment) => Results.Ok(new
{
    name = "Cookbook.Api",
    environment = environment.EnvironmentName,
    status = "ok"
}));

app.MapGet("/health", () => Results.Ok(new
{
    status = "healthy",
    timestampUtc = DateTime.UtcNow
}));

app.MapRecipesEndpoints();

app.Run();

public partial class Program;
