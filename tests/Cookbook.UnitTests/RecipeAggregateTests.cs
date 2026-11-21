using Cookbook.Domain.Entities;
using Cookbook.Domain.Enums;
using Cookbook.Domain.SmartEnums;

namespace Cookbook.UnitTests;

public sealed class RecipeAggregateTests
{
    [Fact]
    public void AddIngredient_AddsRecipeIngredientWithExpectedValues()
    {
        var recipe = new Recipe("Test Recipe",
            Difficulty.Easy,
            CookingProgram.Soup,
            30,
            2, 2);
        var ingredient = new Ingredient("Patate", MeasurementUnit.Gram);

        var added = recipe.AddIngredient(
            ingredient, 250, MeasurementUnit.Gram, "A cubetti");

        Assert.Single(recipe.Ingredients);
        Assert.Equal(ingredient.Id, added.IngredientId);
        Assert.Equal(250m, added.Quantity);
        Assert.Equal("g", added.MeasurementUnit.Symbol);
        Assert.Equal("A cubetti", added.Notes);
    }

    [Fact]
    public void AddStep_AddsStepInAggregate()
    {
        var recipe = new Recipe(
            "Test Recipe",
            Difficulty.Easy,
            CookingProgram.Soup,
            30,
            2,
            3);

        var added = recipe.AddStep("Metti acqua e ingredienti nella pentola.");

        Assert.Single(recipe.Steps);
        Assert.Equal(1, added.StepNumber);
    }
}