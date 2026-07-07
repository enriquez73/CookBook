using Cookbook.Domain.Common;
using Cookbook.Domain.SmartEnums;

namespace Cookbook.Domain.Entities;

/// <summary>
/// Represents an ingredient that can be used in one or more recipes.
/// </summary>
public sealed class Ingredient : AuditableEntity
{
    private readonly List<RecipeIngredient> _recipeIngredients = [];

    /// <summary>
    /// Gets the ingredient name.
    /// </summary>
    public string Name { get; private set; } = string.Empty;

    /// <summary>
    /// Gets the default measurement unit.
    /// </summary>
    public MeasurementUnit DefaultUnit { get; private set; } = null!;

    /// <summary>
    /// Optional notes about the ingredient.
    /// </summary>
    public string? Notes { get; private set; }

    /// <summary>
    /// Gets all recipe associations.
    /// </summary>
    public IReadOnlyCollection<RecipeIngredient> RecipeIngredients =>
        _recipeIngredients.AsReadOnly();
    
    public decimal? Calories { get; private set; }

    public decimal? Proteins { get; private set; }

    public decimal? Carbohydrates { get; private set; }

    public decimal? Fat { get; private set; }

    public decimal? Fiber { get; private set; }

    /// <summary>
    /// Required by Entity Framework.
    /// </summary>
    private Ingredient()
    {
    }

    /// <summary>
    /// Creates a new ingredient.
    /// </summary>
    public Ingredient(
        string name,
        MeasurementUnit defaultUnit,
        string? notes = null)
    {
        Rename(name);
        ChangeDefaultUnit(defaultUnit);
        ChangeNotes(notes);
    }

    /// <summary>
    /// Changes the ingredient name.
    /// </summary>
    public void Rename(string name)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);

        name = name.Trim();

        if (name.Length > 150)
        {
            throw new ArgumentException(
                "Ingredient name cannot exceed 150 characters.",
                nameof(name));
        }

        Name = name;
    }

    /// <summary>
    /// Changes the default measurement unit.
    /// </summary>
    public void ChangeDefaultUnit(MeasurementUnit unit)
    {
        ArgumentNullException.ThrowIfNull(unit);

        DefaultUnit = unit;
    }

    /// <summary>
    /// Changes the ingredient notes.
    /// </summary>
    public void ChangeNotes(string? notes)
    {
        Notes = string.IsNullOrWhiteSpace(notes)
            ? null
            : notes.Trim();
    }

    /// <summary>
    /// Associates this ingredient with a recipe.
    /// Intended for domain logic only.
    /// </summary>
    internal void AddRecipeIngredient(RecipeIngredient recipeIngredient)
    {
        ArgumentNullException.ThrowIfNull(recipeIngredient);

        if (_recipeIngredients.Any(r => r.Id == recipeIngredient.Id))
            return;

        _recipeIngredients.Add(recipeIngredient);
    }

    /// <summary>
    /// Removes the association with a recipe.
    /// Intended for domain logic only.
    /// </summary>
    internal void RemoveRecipeIngredient(RecipeIngredient recipeIngredient)
    {
        ArgumentNullException.ThrowIfNull(recipeIngredient);

        _recipeIngredients.Remove(recipeIngredient);
    }

    public override string ToString() => Name;
}