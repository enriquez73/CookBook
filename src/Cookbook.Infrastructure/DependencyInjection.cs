using Cookbook.Infrastructure.Persistence;
using Cookbook.Application.Common.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Cookbook.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("Default")
            ?? throw new InvalidOperationException(
                "Missing connection string 'ConnectionStrings:Default'. Configure it in appsettings or environment variables.");

        services.AddDbContext<CookbookDbContext>(options =>
            options.UseNpgsql(connectionString));
        services.AddScoped<IApplicationDbContext>(provider =>
            provider.GetRequiredService<CookbookDbContext>());

        return services;
    }
}
