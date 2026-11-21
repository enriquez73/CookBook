using Cookbook.Domain.Common;
using Cookbook.Domain.SmartEnums;

namespace Cookbook.Domain.Entities;

/// <summary>
/// Represents an ingredient used in a recipe.
/// </summary>
public sealed class RecipeIngredient : Entity
{
    /// <summary>
    /// Gets the recipe identifier.
    /// </summary>
    public Guid RecipeId { get; private set; }

    /// <summary>
    /// Gets the parent recipe.
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
    public MeasurementUnit MeasurementUnit { get; private set; } = null!;

    /// <summary>
    /// Gets optional notes.
    /// </summary>
    public string? Notes { get; private set; }
    
    public bool HasNotes =>
        !string.IsNullOrWhiteSpace(Notes);

    /// <summary>
    /// Required by Entity Framework.
    /// </summary>
    private RecipeIngredient()
    {
    }

    internal RecipeIngredient(
        Recipe recipe,
        Ingredient ingredient,
        decimal quantity,
        MeasurementUnit measurementUnit,
        string? notes = null)
    {
        ArgumentNullException.ThrowIfNull(recipe);
        ArgumentNullException.ThrowIfNull(ingredient);
        ArgumentNullException.ThrowIfNull(measurementUnit);

        Recipe = recipe;
        RecipeId = recipe.Id;

        Ingredient = ingredient;
        IngredientId = ingredient.Id;

        ChangeQuantity(quantity);
        ChangeMeasurementUnit(measurementUnit);
        ChangeNotes(notes);
    }

    /// <summary>
    /// Changes the quantity.
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
    public void ChangeMeasurementUnit(
        MeasurementUnit measurementUnit)
    {
        ArgumentNullException.ThrowIfNull(measurementUnit);

        MeasurementUnit = measurementUnit;
    }

    /// <summary>
    /// Changes the notes.
    /// </summary>
    public void ChangeNotes(string? notes)
    {
        if (string.IsNullOrWhiteSpace(notes))
        {
            Notes = null;
            return;
        }

        notes = notes.Trim();

        if (notes.Length > 500)
        {
            throw new ArgumentException(
                "Notes cannot exceed 500 characters.",
                nameof(notes));
        }

        Notes = notes;
    }

    public override string ToString()
        => $"{Quantity} {MeasurementUnit.Symbol} {Ingredient.Name}";
}