using Cookbook.Application.Common.Interfaces;
using Cookbook.Domain.Common;
using Cookbook.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Cookbook.Infrastructure.Persistence;

public sealed class CookbookDbContext : DbContext, IApplicationDbContext
{
    public CookbookDbContext(DbContextOptions<CookbookDbContext> options)
        : base(options)
    {
    }

    public DbSet<Recipe> Recipes => Set<Recipe>();

    public DbSet<Ingredient> Ingredients => Set<Ingredient>();

    public DbSet<Category> Categories => Set<Category>();

    public DbSet<Tag> Tags => Set<Tag>();

    public DbSet<Equipment> Equipments => Set<Equipment>();

    public DbSet<RecipeIngredient> RecipeIngredients => Set<RecipeIngredient>();

    public DbSet<RecipeStep> RecipeSteps => Set<RecipeStep>();

    public DbSet<RecipeImage> RecipeImages => Set<RecipeImage>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Ignore<DomainEvent>();

        modelBuilder.ApplyConfigurationsFromAssembly(InfrastructureAssemblyReference.Assembly);
    }

    public override Task<int> SaveChangesAsync(
        CancellationToken cancellationToken = default)
    {
        SetAuditValues();

        return base.SaveChangesAsync(cancellationToken);
    }

    private void SetAuditValues()
    {
        var now = DateTime.UtcNow;

        foreach (var entry in ChangeTracker.Entries<AuditableEntity>())
        {
            if (entry.State == EntityState.Added)
                entry.Property(x => x.CreatedAt).CurrentValue = now;

            if (entry.State == EntityState.Modified)
                entry.Property(x => x.UpdatedAt).CurrentValue = now;
        }
    }
}