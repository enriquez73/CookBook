using Cookbook.Domain.Enums;
using MediatR;

namespace Cookbook.Application.Recipes.Commands.UpdateRecipe;

public sealed record UpdateRecipeCommand(
    Guid Id,
    string Name,
    string? Description,
    Difficulty Difficulty,
    CookingProgram CookingProgram,
    int PreparationTimeMinutes,
    int CookingTimeMinutes,
    int Servings,
    string? Notes,
    string? Source)
    : IRequest;