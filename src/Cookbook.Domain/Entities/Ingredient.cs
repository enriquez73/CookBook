using Cookbook.Domain.Common;
using Cookbook.Domain.SmartEnums;
using Cookbook.Domain.ValueObjects;

namespace Cookbook.Domain.Entities;

/// <summary>
/// Represents an ingredient that can be used in recipes.
/// </summary>
public sealed class Ingredient : AuditableEntity
{
    /// <summary>
    /// Gets the ingredient name.
    /// </summary>
    public string Name { get; private set; } = string.Empty;

    /// <summary>
    /// Gets the ingredient description.
    /// </summary>
    public string? Description { get; private set; }

    /// <summary>
    /// Gets the default measurement unit.
    /// </summary>
    public MeasurementUnit DefaultMeasurementUnit { get; private set; } = null!;

    /// <summary>
    /// Gets the nutritional values per 100 g or 100 ml.
    /// </summary>
    public NutritionFacts? Nutrition { get; private set; }

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
        MeasurementUnit defaultMeasurementUnit,
        string? description = null,
        NutritionFacts? nutrition = null)
    {
        Rename(name);
        ChangeDefaultMeasurementUnit(defaultMeasurementUnit);
        ChangeDescription(description);

        Nutrition = nutrition;
    }

    /// <summary>
    /// Renames the ingredient.
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
    /// Changes the ingredient description.
    /// </summary>
    public void ChangeDescription(string? description)
    {
        if (string.IsNullOrWhiteSpace(description))
        {
            Description = null;
            return;
        }

        description = description.Trim();

        if (description.Length > 1000)
        {
            throw new ArgumentException(
                "Ingredient description cannot exceed 1000 characters.",
                nameof(description));
        }

        Description = description;
    }

    /// <summary>
    /// Changes the default measurement unit.
    /// </summary>
    public void ChangeDefaultMeasurementUnit(
        MeasurementUnit measurementUnit)
    {
        ArgumentNullException.ThrowIfNull(measurementUnit);

        DefaultMeasurementUnit = measurementUnit;
    }

    /// <summary>
    /// Updates the nutritional values.
    /// </summary>
    public void ChangeNutrition(
        NutritionFacts nutrition)
    {
        ArgumentNullException.ThrowIfNull(nutrition);

        Nutrition = nutrition;
    }

    /// <summary>
    /// Removes the nutritional values.
    /// </summary>
    public void RemoveNutrition()
    {
        Nutrition = null;
    }

    public override string ToString()
        => Name;
}