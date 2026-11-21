using Cookbook.Application.Common.Interfaces;
using Cookbook.Application.Common.Models;
using Cookbook.Application.Recipes.DTOs;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Cookbook.Application.Recipes.Queries.GetRecipes;

public sealed class GetRecipesQueryHandler
    : IRequestHandler<GetRecipesQuery, PagedResult<RecipeListItemDto>>
{
    private readonly IApplicationDbContext _context;

    public GetRecipesQueryHandler(
        IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<PagedResult<RecipeListItemDto>> Handle(
        GetRecipesQuery request,
        CancellationToken cancellationToken)
    {
        var query = _context.Recipes
            .AsNoTracking();

        var totalCount = await query.CountAsync(cancellationToken);

        var items = await query
            .OrderBy(x => x.Name)
            .Skip((request.Page - 1) * request.PageSize)
            .Take(request.PageSize)
            .Select(x => new RecipeListItemDto(
                x.Id,
                x.Name,
                x.Difficulty,
                x.CookingProgram,
                x.PreparationTimeMinutes,
                x.CookingTimeMinutes,
                x.Servings,
                x.Images
                    .OrderBy(i => i.SortOrder)
                    .Where(i => i.IsPrimary)
                    .Select(i => i.StoredFileName)
                    .FirstOrDefault()))
            .ToListAsync(cancellationToken);

        return new PagedResult<RecipeListItemDto>(
            items,
            request.Page,
            request.PageSize,
            totalCount);
    }
}