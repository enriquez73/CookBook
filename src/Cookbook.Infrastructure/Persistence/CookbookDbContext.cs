using Cookbook.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Cookbook.Infrastructure.Persistence;

public sealed class CookbookDbContext : DbContext
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

        modelBuilder.ApplyConfigurationsFromAssembly(typeof(CookbookDbContext).Assembly);
    }
}