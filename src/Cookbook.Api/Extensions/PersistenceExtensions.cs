using Cookbook.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Cookbook.Api.Extensions;

public static class PersistenceExtensions
{
    public static async Task ApplyDatabaseMigrationsAsync(
        this WebApplication app)
    {
        await using var scope = app.Services.CreateAsyncScope();

        var context = scope.ServiceProvider
            .GetRequiredService<CookbookDbContext>();
        await context.Database.MigrateAsync();
    }
}