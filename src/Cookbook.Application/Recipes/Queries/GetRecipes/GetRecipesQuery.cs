using Cookbook.Application.Common.Models;
using Cookbook.Application.Recipes.DTOs;
using MediatR;

namespace Cookbook.Application.Recipes.Queries.GetRecipes;

public sealed record GetRecipesQuery(
    int Page = 1,
    int PageSize = 20)
    : IRequest<PagedResult<RecipeListItemDto>>;