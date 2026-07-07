using Cookbook.Domain.Common;
using Cookbook.Domain.SmartEnums;

namespace Cookbook.Domain.Entities;

/// <summary>
/// Represents an ingredient used by a recipe.
/// </summary>
public sealed class RecipeIngredient : AuditableEntity
{
    /// <summary>
    /// Gets the recipe identifier.
    /// </summary>
    public Guid RecipeId { get; private set; }

    /// <summary>
    /// Gets the recipe.
    /// </summary>
    public Recipe Recipe { get; private set; } = null!;

    /// <summary>
    /// Gets the ingredient identifier.
    /// </summary>
    public Guid IngredientId { get; private set; }

    /// <summary>
    /// Gets the ingredient.
    /// </summary>
    public Ingredient Ingredient { get; private set; } = null!;

    /// <summary>
    /// Gets the quantity.
    /// </summary>
    public decimal Quantity { get; private set; }

    /// <summary>
    /// Gets the measurement unit.
    /// </summary>
    public MeasurementUnit Unit { get; private set; } = null!;

    /// <summary>
    /// Optional notes.
    /// </summary>
    public string? Notes { get; private set; }

    /// <summary>
    /// Required by Entity Framework.
    /// </summary>
    private RecipeIngredient()
    {
    }

    /// <summary>
    /// Creates a new recipe ingredient.
    /// Only the Recipe aggregate root should create instances.
    /// </summary>
    internal RecipeIngredient(
        Recipe recipe,
        Ingredient ingredient,
        decimal quantity,
        MeasurementUnit unit,
        string? notes = null)
    {
        ArgumentNullException.ThrowIfNull(recipe);
        ArgumentNullException.ThrowIfNull(ingredient);
        ArgumentNullException.ThrowIfNull(unit);

        Recipe = recipe;
        RecipeId = recipe.Id;

        Ingredient = ingredient;
        IngredientId = ingredient.Id;

        ChangeQuantity(quantity);
        ChangeUnit(unit);
        ChangeNotes(notes);
    }

    /// <summary>
    /// Changes the ingredient quantity.
    /// </summary>
    public void ChangeQuantity(decimal quantity)
    {
        if (quantity <= 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(quantity),
                "Quantity must be greater than zero.");
        }

        Quantity = quantity;
    }

    /// <summary>
    /// Changes the measurement unit.
    /// </summary>
    public void ChangeUnit(MeasurementUnit unit)
    {
        ArgumentNullException.ThrowIfNull(unit);

        Unit = unit;
    }

    /// <summary>
    /// Changes the notes.
    /// </summary>
    public void ChangeNotes(string? notes)
    {
        Notes = string.IsNullOrWhiteSpace(notes)
            ? null
            : notes.Trim();
    }

    public override string ToString()
        => $"{Quantity} {Unit.Symbol} {Ingredient.Name}";
}