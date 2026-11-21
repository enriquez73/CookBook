using Cookbook.Domain.Enums;

namespace Cookbook.Application.Recipes.DTOs;

public sealed record RecipeDto(
    Guid Id,
    string Name,
    string? Description,
    Difficulty Difficulty,
    CookingProgram CookingProgram,
    int PreparationTimeMinutes,
    int CookingTimeMinutes,
    int Servings,
    string? Notes,
    string? Source);