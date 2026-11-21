using Cookbook.Application.Common.Exceptions;
using Cookbook.Application.Common.Interfaces;
using Cookbook.Application.Recipes.DTOs;
using Cookbook.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Cookbook.Application.Recipes.Queries.GetRecipe;

public sealed class GetRecipeQueryHandler
    : IRequestHandler<GetRecipeQuery, RecipeDto>
{
    private readonly IApplicationDbContext _context;

    public GetRecipeQueryHandler(
        IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<RecipeDto> Handle(
        GetRecipeQuery request,
        CancellationToken cancellationToken)
    {
        var recipe = await _context.Recipes
            .AsNoTracking()
            .Where(x => x.Id == request.Id)
            .Select(x => new RecipeDto(
                x.Id,
                x.Name,
                x.Description,
                x.Difficulty,
                x.CookingProgram,
                x.PreparationTimeMinutes,
                x.CookingTimeMinutes,
                x.Servings,
                x.Notes,
                x.Source))
            .SingleOrDefaultAsync(cancellationToken);

        if (recipe is null)
            throw new NotFoundException(nameof(Recipe), request.Id);

        return recipe;
    }
}