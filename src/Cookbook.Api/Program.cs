using Cookbook.Api.Endpoints;
using Cookbook.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddProblemDetails();
builder.Services.AddOpenApi();
builder.Services.AddInfrastructure(builder.Configuration);

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
	app.MapOpenApi();
}

app.UseExceptionHandler();


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
