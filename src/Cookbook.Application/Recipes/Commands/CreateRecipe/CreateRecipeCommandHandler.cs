using Cookbook.Application.Common.Interfaces;
using Cookbook.Domain.Entities;
using MediatR;

namespace Cookbook.Application.Recipes.Commands.CreateRecipe;

public sealed class CreateRecipeCommandHandler
    : IRequestHandler<CreateRecipeCommand, CreateRecipeResponse>
{
    private readonly IApplicationDbContext _context;

    public CreateRecipeCommandHandler(
        IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<CreateRecipeResponse> Handle(
        CreateRecipeCommand request,
        CancellationToken cancellationToken)
    {
        /*
        public Recipe(
           string name, 
           Difficulty difficulty, 
           CookingProgram cookingProgram, 
           int preparationTimeMinutes, 
           int cookingTimeMinutes, 
           int servings, 
           string? description = null, 
           string? notes = null, 
           string? source = null)
        */
        var recipe = new Recipe(
            request.Name,
            request.Difficulty,
            request.CookingProgram,
            request.PreparationTimeMinutes,
            request.CookingTimeMinutes,
            request.Servings,
            request.Description,
            request.Notes,
            request.Source);

        await _context.Recipes.AddAsync(
            recipe,
            cancellationToken);

        await _context.SaveChangesAsync(
            cancellationToken);

        return new CreateRecipeResponse(recipe.Id);
    }
}