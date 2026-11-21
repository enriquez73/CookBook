using Cookbook.Application.Recipes.DTOs;
using MediatR;

namespace Cookbook.Application.Recipes.Queries.GetRecipe;

public sealed record GetRecipeQuery(Guid Id)
    : IRequest<RecipeDto>;