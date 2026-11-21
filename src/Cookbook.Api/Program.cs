using Cookbook.Api.Endpoints;
using Cookbook.Api.Extensions;
using Cookbook.Application;
using Cookbook.Infrastructure;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services
	.AddApplication()
	.AddInfrastructure(builder.Configuration)
	.AddApi()
	.AddEndpoints();

var app = builder.Build();

app.UseExceptionHandler();

app.MapGet("/", () => Results.Ok(new { Name = "Cookbook API" }));
app.MapGet("/health", () => Results.Ok());

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();

    app.MapScalarApiReference(options =>
    {
        options
            .WithTitle("Cookbook API")
            .WithTheme(ScalarTheme.Kepler)
            .WithDefaultHttpClient(ScalarTarget.CSharp, ScalarClient.HttpClient);
    });
}

if (!app.Environment.IsEnvironment("Testing"))
    await app.ApplyDatabaseMigrationsAsync();

app.MapEndpoints();

await app.RunAsync();

public partial class Program;
