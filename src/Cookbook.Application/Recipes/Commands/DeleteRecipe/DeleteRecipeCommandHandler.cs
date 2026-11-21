using Cookbook.Application.Common.Exceptions;
using Cookbook.Application.Common.Interfaces;
using Cookbook.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Cookbook.Application.Recipes.Commands.DeleteRecipe;

public sealed class DeleteRecipeCommandHandler
    : IRequestHandler<DeleteRecipeCommand>
{
    private readonly IApplicationDbContext _context;

    public DeleteRecipeCommandHandler(
        IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task Handle(
        DeleteRecipeCommand request,
        CancellationToken cancellationToken)
    {
        var recipe = await _context.Recipes
            .SingleOrDefaultAsync(
                x => x.Id == request.Id,
                cancellationToken);

        if (recipe is null)
            throw new NotFoundException(nameof(Recipe), request.Id);

        _context.Recipes.Remove(recipe);

        await _context.SaveChangesAsync(cancellationToken);
    }
}