using Cookbook.Domain.Enums;
using MediatR;

namespace Cookbook.Application.Recipes.Commands.CreateRecipe;

public sealed record CreateRecipeCommand(
    string Name,
    string? Description,
    Difficulty Difficulty,
    CookingProgram CookingProgram,
    int PreparationTimeMinutes,
    int CookingTimeMinutes,
    int Servings,
    string? Notes,
    string? Source)
    : IRequest<CreateRecipeResponse>;