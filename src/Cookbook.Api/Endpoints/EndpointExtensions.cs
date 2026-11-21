using System.Reflection;

namespace Cookbook.Api.Endpoints;

public static class EndpointExtensions
{
    public static IServiceCollection AddEndpoints(
        this IServiceCollection services)
    {
        var endpointTypes = typeof(IEndpoint).Assembly
            .DefinedTypes
            .Where(t =>
                t is { IsAbstract: false, IsInterface: false } &&
                typeof(IEndpoint).IsAssignableFrom(t));

        foreach (var endpointType in endpointTypes)
        {
            services.AddSingleton(typeof(IEndpoint), endpointType);
        }

        return services;
    }

    public static WebApplication MapEndpoints(
        this WebApplication app)
    {
        var endpoints = app.Services.GetServices<IEndpoint>();

        foreach (var endpoint in endpoints)
        {
            endpoint.MapEndpoint(app);
        }

        return app;
    }
}