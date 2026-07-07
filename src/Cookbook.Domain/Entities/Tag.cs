using Cookbook.Domain.Common;

namespace Cookbook.Domain.Entities;

/// <summary>
/// Represents a tag assigned to recipes.
/// </summary>
public sealed class Tag : AuditableEntity
{
    private readonly List<Recipe> _recipes = [];

    /// <summary>
    /// Gets the tag name.
    /// </summary>
    public string Name { get; private set; } = string.Empty;

    /// <summary>
    /// Gets the recipes associated with this tag.
    /// </summary>
    public IReadOnlyCollection<Recipe> Recipes => _recipes.AsReadOnly();

    /// <summary>
    /// Required by Entity Framework.
    /// </summary>
    private Tag()
    {
    }

    /// <summary>
    /// Creates a new tag.
    /// </summary>
    public Tag(string name)
    {
        Rename(name);
    }

    /// <summary>
    /// Renames the tag.
    /// </summary>
    public void Rename(string name)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);

        name = name.Trim();

        if (name.Length > 100)
            throw new ArgumentException(
                "Tag name cannot exceed 100 characters.",
                nameof(name));

        Name = name;
    }

    /// <summary>
    /// Associates a recipe with this tag.
    /// </summary>
    internal void AddRecipe(Recipe recipe)
    {
        ArgumentNullException.ThrowIfNull(recipe);

        if (_recipes.Any(r => r.Id == recipe.Id))
            return;

        _recipes.Add(recipe);
    }

    /// <summary>
    /// Removes a recipe from this tag.
    /// </summary>
    internal void RemoveRecipe(Recipe recipe)
    {
        ArgumentNullException.ThrowIfNull(recipe);

        _recipes.Remove(recipe);
    }

    public override string ToString() => Name;
}