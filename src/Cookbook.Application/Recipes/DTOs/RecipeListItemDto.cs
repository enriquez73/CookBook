using Cookbook.Domain.Enums;

namespace Cookbook.Application.Recipes.DTOs;

public sealed record RecipeListItemDto(
    Guid Id,
    string Name,
    Difficulty Difficulty,
    CookingProgram CookingProgram,
    int PreparationTimeMinutes,
    int CookingTimeMinutes,
    int Servings,
    string? Thumbnail);