using Cookbook.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Cookbook.Infrastructure;

public static class DependencyInjection
{
    /// <summary>
    /// Database:Provider = Sqlite (default) oppure Postgres. ConnectionStrings:Default è la connection string.
    /// </summary>
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        var provider = configuration["Database:Provider"] ?? "Sqlite";
        var connectionString = configuration.GetConnectionString("Default");

        services.AddDbContext<CookbookDbContext>(options =>
        {
            if (provider.Equals("Postgres", StringComparison.OrdinalIgnoreCase))
            {
                options.UseNpgsql(connectionString
                    ?? throw new InvalidOperationException("Manca ConnectionStrings:Default per Postgres."));
            }
            else
            {
                options.UseSqlite(connectionString ?? "Data Source=cookbook.db");
            }
        });

        return services;
    }
}
