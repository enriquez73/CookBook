using Cookbook.Domain.Entities;
using Cookbook.Domain.Enums;

namespace Cookbook.UnitTests;

public sealed class RecipeAggregateTests
{
    [Fact]
    public void AddIngredient_AddsRecipeIngredientWithExpectedValues()
    {
        var recipe = new Recipe("Test Recipe", Difficulty.Easy, CookingProgram.Soup, 30, 2);
        var ingredient = new Ingredient("Patate", "g");

        var added = recipe.AddIngredient(ingredient, 250m, "g", "A cubetti");

        Assert.Single(recipe.Ingredients);
        Assert.Equal(ingredient.Id, added.IngredientId);
        Assert.Equal(250m, added.Quantity);
        Assert.Equal("g", added.Unit);
        Assert.Equal("A cubetti", added.Notes);
    }

    [Fact]
    public void AddStep_AddsStepInAggregate()
    {
        var recipe = new Recipe("Test Recipe", Difficulty.Easy, CookingProgram.Soup, 30, 2);

        var added = recipe.AddStep(1, "Metti acqua e ingredienti nella pentola.");

        Assert.Single(recipe.Steps);
        Assert.Equal(1, added.StepNumber);
    }
}

