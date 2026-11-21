using Cookbook.Api.Middleware;
using Microsoft.AspNetCore.Http.Json;

namespace Cookbook.Api.Extensions;

public static class ApiExtensions
{
    public static IServiceCollection AddApi(
        this IServiceCollection services)
    {
        services.AddOpenApi();

        services.Configure<JsonOptions>(options =>
        {
            options.SerializerOptions.PropertyNamingPolicy = null;
            options.SerializerOptions.WriteIndented = true;
            
        });
        services.AddExceptionHandler<GlobalExceptionHandler>();
        services.AddProblemDetails();

        return services;
    }
}