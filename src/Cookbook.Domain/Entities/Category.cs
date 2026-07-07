using Cookbook.Domain.Common;

namespace Cookbook.Domain.Entities;

/// <summary>
/// Represents a recipe category.
/// </summary>
public sealed class Category : AuditableEntity
{
    private readonly List<Recipe> _recipes = [];

    /// <summary>
    /// Gets the category name.
    /// </summary>
    public string Name { get; private set; } = string.Empty;

    /// <summary>
    /// Gets the recipes belonging to this category.
    /// </summary>
    public IReadOnlyCollection<Recipe> Recipes => _recipes.AsReadOnly();

    /// <summary>
    /// Required by Entity Framework.
    /// </summary>
    private Category()
    {
    }

    /// <summary>
    /// Creates a new category.
    /// </summary>
    public Category(string name)
    {
        Rename(name);
    }

    /// <summary>
    /// Renames the category.
    /// </summary>
    public void Rename(string name)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);

        name = name.Trim();

        if (name.Length > 100)
            throw new ArgumentException(
                "Category name cannot exceed 100 characters.",
                nameof(name));

        Name = name;
    }

    /// <summary>
    /// Associates a recipe with this category.
    /// </summary>
    internal void AddRecipe(Recipe recipe)
    {
        ArgumentNullException.ThrowIfNull(recipe);

        if (_recipes.Any(r => r.Id == recipe.Id))
            return;

        _recipes.Add(recipe);
    }

    /// <summary>
    /// Removes a recipe from this category.
    /// </summary>
    internal void RemoveRecipe(Recipe recipe)
    {
        ArgumentNullException.ThrowIfNull(recipe);

        _recipes.Remove(recipe);
    }

    public override string ToString() => Name;
}