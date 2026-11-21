using Cookbook.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Cookbook.Application.Common.Interfaces;

public interface IApplicationDbContext
{
    DbSet<Recipe> Recipes { get; }

    DbSet<Ingredient> Ingredients { get; }

    DbSet<Category> Categories { get; }

    DbSet<Tag> Tags { get; }

    DbSet<Equipment> Equipments { get; }

    DbSet<RecipeImage> RecipeImages { get; }

    DbSet<RecipeIngredient> RecipeIngredients { get; }

    DbSet<RecipeStep> RecipeSteps { get; }

    Task<int> SaveChangesAsync(
        CancellationToken cancellationToken = default);
}