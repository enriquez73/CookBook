using Cookbook.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Cookbook.Infrastructure.Persistence;

public sealed class CookbookDbContext(DbContextOptions<CookbookDbContext> options) : DbContext(options)
{
    public DbSet<Recipe> Recipes => Set<Recipe>();
    public DbSet<Tag> Tags => Set<Tag>();

    protected override void OnModelCreating(ModelBuilder b)
    {
        b.Entity<Recipe>(e =>
        {
            e.Property(x => x.Title).HasMaxLength(200).IsRequired();
            e.Property(x => x.Description).HasMaxLength(4000);
            e.Property(x => x.CookerFunction).HasConversion<string>().HasMaxLength(30);
            e.Property(x => x.RiceCups).HasConversion<double>();
            e.HasMany(x => x.Ingredients).WithOne().HasForeignKey(x => x.RecipeId).OnDelete(DeleteBehavior.Cascade);
            e.HasMany(x => x.Steps).WithOne().HasForeignKey(x => x.RecipeId).OnDelete(DeleteBehavior.Cascade);
            e.HasMany(x => x.Media).WithOne().HasForeignKey(x => x.RecipeId).OnDelete(DeleteBehavior.Cascade);
            e.HasMany(x => x.Tags).WithMany(x => x.Recipes);
        });
        b.Entity<RecipeIngredient>().Property(x => x.Name).HasMaxLength(200).IsRequired();
        b.Entity<RecipeIngredient>().Property(x => x.Quantity).HasMaxLength(100);
        b.Entity<RecipeStep>().Property(x => x.Text).HasMaxLength(4000).IsRequired();
        b.Entity<Tag>(e =>
        {
            e.Property(x => x.Name).HasMaxLength(50).IsRequired();
            e.HasIndex(x => x.Name).IsUnique();
        });
        b.Entity<RecipeMedia>(e =>
        {
            e.Property(x => x.Kind).HasConversion<string>().HasMaxLength(20);
            e.Property(x => x.Url).HasMaxLength(2000);
            e.Property(x => x.Caption).HasMaxLength(300);
        });
    }
}
