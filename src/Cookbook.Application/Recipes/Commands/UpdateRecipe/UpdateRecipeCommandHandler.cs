using Cookbook.Application.Common.Exceptions;
using Cookbook.Application.Common.Interfaces;
using Cookbook.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Cookbook.Application.Recipes.Commands.UpdateRecipe;

public sealed class UpdateRecipeCommandHandler
    : IRequestHandler<UpdateRecipeCommand>
{
    private readonly IApplicationDbContext _context;

    public UpdateRecipeCommandHandler(
        IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task Handle(
        UpdateRecipeCommand request,
        CancellationToken cancellationToken)
    {
        var recipe = await _context.Recipes
            .SingleOrDefaultAsync(
                x => x.Id == request.Id,
                cancellationToken);

        if (recipe is null)
            throw new NotFoundException(nameof(Recipe), request.Id);

        recipe.Update(
            request.Name,
            request.Description,
            request.Difficulty,
            request.CookingProgram,
            request.PreparationTimeMinutes,
            request.CookingTimeMinutes,
            request.Servings,
            request.Notes,
            request.Source);

        await _context.SaveChangesAsync(cancellationToken);
    }
}