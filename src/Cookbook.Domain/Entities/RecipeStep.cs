using Cookbook.Domain.Common;

namespace Cookbook.Domain.Entities;

/// <summary>
/// Represents a single preparation step of a recipe.
/// </summary>
public sealed class RecipeStep : Entity
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
    /// Gets the step number.
    /// </summary>
    public int StepNumber { get; private set; }

    /// <summary>
    /// Gets the step description.
    /// </summary>
    public string Description { get; private set; } = string.Empty;

    /// <summary>
    /// Required by Entity Framework.
    /// </summary>
    private RecipeStep()
    {
    }

    internal RecipeStep(
        Recipe recipe,
        int stepNumber,
        string description)
    {
        ArgumentNullException.ThrowIfNull(recipe);

        Recipe = recipe;
        RecipeId = recipe.Id;

        ChangeStepNumber(stepNumber);
        ChangeDescription(description);
    }

    /// <summary>
    /// Changes the step description.
    /// </summary>
    public void ChangeDescription(string description)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(description);

        description = description.Trim();

        if (description.Length > 4000)
        {
            throw new ArgumentException(
                "Step description cannot exceed 4000 characters.",
                nameof(description));
        }

        Description = description;
    }

    /// <summary>
    /// Changes the step number.
    /// Intended to be used only by the Recipe aggregate.
    /// </summary>
    internal void ChangeStepNumber(int stepNumber)
    {
        ArgumentOutOfRangeException.ThrowIfLessThan(stepNumber, 1);

        StepNumber = stepNumber;
    }

    public override string ToString()
        => $"Step {StepNumber}";
}