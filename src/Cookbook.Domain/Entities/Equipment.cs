using Cookbook.Domain.Common;

namespace Cookbook.Domain.Entities;

/// <summary>
/// Represents a piece of equipment used to prepare recipes.
/// </summary>
public sealed class Equipment : AuditableEntity
{
    private readonly List<Recipe> _recipes = [];

    /// <summary>
    /// Gets the equipment name.
    /// </summary>
    public string Name { get; private set; } = string.Empty;

    /// <summary>
    /// Gets the recipes using this equipment.
    /// </summary>
    public IReadOnlyCollection<Recipe> Recipes => _recipes.AsReadOnly();

    /// <summary>
    /// Required by Entity Framework.
    /// </summary>
    private Equipment()
    {
    }

    /// <summary>
    /// Creates a new equipment.
    /// </summary>
    public Equipment(string name)
    {
        Rename(name);
    }

    /// <summary>
    /// Renames the equipment.
    /// </summary>
    public void Rename(string name)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);

        name = name.Trim();

        if (name.Length > 100)
            throw new ArgumentException(
                "Equipment name cannot exceed 100 characters.",
                nameof(name));

        Name = name;
    }

    /// <summary>
    /// Associates a recipe with this equipment.
    /// </summary>
    internal void AddRecipe(Recipe recipe)
    {
        ArgumentNullException.ThrowIfNull(recipe);

        if (_recipes.Any(r => r.Id == recipe.Id))
            return;

        _recipes.Add(recipe);
    }

    /// <summary>
    /// Removes a recipe from this equipment.
    /// </summary>
    internal void RemoveRecipe(Recipe recipe)
    {
        ArgumentNullException.ThrowIfNull(recipe);

        _recipes.Remove(recipe);
    }

    public override string ToString() => Name;
}