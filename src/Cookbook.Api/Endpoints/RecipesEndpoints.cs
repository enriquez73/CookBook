using Cookbook.Domain.Entities;
using Cookbook.Domain.Enums;
using Cookbook.Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace Cookbook.Api.Endpoints;

public static class RecipesEndpoints
{
    public static IEndpointRouteBuilder MapRecipesEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/recipes");

        group.MapGet("/", GetRecipesAsync);
        group.MapGet("/{id:guid}", GetRecipeByIdAsync);
        group.MapPost("/seed/japanese-curry", SeedJapaneseCurryAsync);

        return app;
    }

    private static async Task<IResult> GetRecipesAsync(CookbookDbContext db, CancellationToken cancellationToken)
    {
        var recipes = await db.Recipes
            .AsNoTracking()
            .OrderBy(recipe => recipe.Name)
            .Select(recipe => new RecipeListItemResponse(
                recipe.Id,
                recipe.Name,
                recipe.Description,
                recipe.Difficulty.ToString(),
                recipe.CookingProgram.ToString(),
                recipe.CookingTimeMinutes,
                recipe.Servings))
            .ToListAsync(cancellationToken);

        return Results.Ok(recipes);
    }

    private static async Task<IResult> GetRecipeByIdAsync(Guid id, CookbookDbContext db, CancellationToken cancellationToken)
    {
        var recipe = await db.Recipes
            .AsNoTracking()
            .Include(recipe => recipe.Ingredients)
            .ThenInclude(recipeIngredient => recipeIngredient.Ingredient)
            .Include(recipe => recipe.Steps)
            .FirstOrDefaultAsync(recipe => recipe.Id == id, cancellationToken);

        if (recipe is null)
        {
            return Results.NotFound();
        }

        var response = new RecipeDetailsResponse(
            recipe.Id,
            recipe.Name,
            recipe.Description,
            recipe.Difficulty.ToString(),
            recipe.CookingProgram.ToString(),
            recipe.CookingTimeMinutes,
            recipe.Servings,
            recipe.Ingredients
                .OrderBy(item => item.Ingredient.Name)
                .Select(item => new RecipeIngredientResponse(
                    item.Ingredient.Name,
                    item.Quantity,
                    item.Unit,
                    item.Notes))
                .ToArray(),
            recipe.Steps
                .OrderBy(step => step.StepNumber)
                .Select(step => new RecipeStepResponse(step.StepNumber, step.Description))
                .ToArray());

        return Results.Ok(response);
    }

    private static async Task<IResult> SeedJapaneseCurryAsync(CookbookDbContext db, CancellationToken cancellationToken)
    {
        await db.Database.EnsureCreatedAsync(cancellationToken);

        const string recipeName = "021 - Curry Giapponese (Japanese Curry Rice)";

        var existing = await db.Recipes
            .AsNoTracking()
            .AnyAsync(recipe => recipe.Name == recipeName, cancellationToken);

        if (existing)
        {
            return Results.Ok(new { message = "Recipe already seeded." });
        }

        var recipe = new Recipe(
            name: recipeName,
            difficulty: Difficulty.Easy,
            cookingProgram: CookingProgram.Soup,
            cookingTimeMinutes: 45,
            servings: 2,
            description:
            "Curry giapponese denso e delicato preparato nel cuociriso Bear; da servire con riso jasmine cotto separatamente in programma White Rice.");

        var chicken = await GetOrCreateIngredientAsync(db, "Petto di pollo", "g", cancellationToken);
        var potatoes = await GetOrCreateIngredientAsync(db, "Patate", "g", cancellationToken);
        var carrots = await GetOrCreateIngredientAsync(db, "Carote", "g", cancellationToken);
        var onion = await GetOrCreateIngredientAsync(db, "Cipolla grande", "g", cancellationToken);
        var water = await GetOrCreateIngredientAsync(db, "Acqua", "ml", cancellationToken);
        var curryBlocks = await GetOrCreateIngredientAsync(db, "Curry giapponese in blocchi", "g", cancellationToken);
        var worcestershire = await GetOrCreateIngredientAsync(db, "Salsa Worcestershire", "ml", cancellationToken);
        var soy = await GetOrCreateIngredientAsync(db, "Salsa di soia", "ml", cancellationToken);
        var honey = await GetOrCreateIngredientAsync(db, "Miele", "g", cancellationToken);
        var butter = await GetOrCreateIngredientAsync(db, "Burro", "g", cancellationToken);
        var jasmineRice = await GetOrCreateIngredientAsync(db, "Riso Jasmine", "g", cancellationToken);
        var riceWater = await GetOrCreateIngredientAsync(db, "Acqua per riso", "ml", cancellationToken);

        recipe.AddIngredient(chicken, 300m, "g", "Cubetti da circa 3 cm");
        recipe.AddIngredient(potatoes, 250m, "g", "Cubi da circa 2 cm");
        recipe.AddIngredient(carrots, 150m, "g", "Tagliare a rondelle");
        recipe.AddIngredient(onion, 150m, "g", "Fettine sottili");
        recipe.AddIngredient(water, 500m, "ml");
        recipe.AddIngredient(curryBlocks, 40m, "g", "Circa 2 blocchi Golden Curry medio piccante");
        recipe.AddIngredient(worcestershire, 10m, "ml");
        recipe.AddIngredient(soy, 10m, "ml");
        recipe.AddIngredient(honey, 10m, "g");
        recipe.AddIngredient(butter, 10m, "g");
        recipe.AddIngredient(jasmineRice, 200m, "g", "Preparare separatamente");
        recipe.AddIngredient(riceWater, 280m, "ml", "Per il programma White Rice");

        recipe.AddStep(1,
            "Taglia pollo, patate, carote e cipolla; inserisci nella pentola con 500 ml di acqua senza aggiungere il curry.");
        recipe.AddStep(2, "Avvia il programma Soup e lascia cuocere circa 30 minuti.");
        recipe.AddStep(3,
            "Apri il coperchio, aggiungi curry, salsa di soia, Worcestershire, miele e burro; mescola fino a scioglimento completo.");
        recipe.AddStep(4, "Richiudi e prosegui la cottura per altri 10-15 minuti fino a ottenere una salsa cremosa.");
        recipe.AddStep(5,
            "In parallelo cuoci 200 g di riso jasmine con 280 ml di acqua nel programma White Rice e servi riso e curry separati nel piatto.");

        db.Recipes.Add(recipe);
        await db.SaveChangesAsync(cancellationToken);

        return Results.Created($"/api/recipes/{recipe.Id}", new { recipe.Id, recipe.Name });
    }

    private static async Task<Ingredient> GetOrCreateIngredientAsync(
        CookbookDbContext db,
        string name,
        string defaultUnit,
        CancellationToken cancellationToken)
    {
        var normalizedName = name.Trim();

        var existing = await db.Ingredients.FirstOrDefaultAsync(
            ingredient => ingredient.Name == normalizedName,
            cancellationToken);

        if (existing is not null)
        {
            return existing;
        }

        var ingredient = new Ingredient(normalizedName, defaultUnit);
        db.Ingredients.Add(ingredient);
        return ingredient;
    }

    private sealed record RecipeListItemResponse(
        Guid Id,
        string Name,
        string? Description,
        string Difficulty,
        string CookingProgram,
        int CookingTimeMinutes,
        int Servings);

    private sealed record RecipeDetailsResponse(
        Guid Id,
        string Name,
        string? Description,
        string Difficulty,
        string CookingProgram,
        int CookingTimeMinutes,
        int Servings,
        IReadOnlyCollection<RecipeIngredientResponse> Ingredients,
        IReadOnlyCollection<RecipeStepResponse> Steps);

    private sealed record RecipeIngredientResponse(string Name, decimal Quantity, string Unit, string? Notes);

    private sealed record RecipeStepResponse(int StepNumber, string Description);
}


