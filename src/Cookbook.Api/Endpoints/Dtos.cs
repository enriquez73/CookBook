using Cookbook.Domain.Entities;

namespace Cookbook.Api.Endpoints;

public sealed record IngredientDto(string Name, string? Quantity);

public sealed record MediaDto(Guid Id, MediaKind Kind, string Url, string? Caption);

public sealed record RecipeRequest(
    string Title,
    string? Description,
    int Servings,
    decimal RiceCups,
    int CookingTimeMinutes,
    CookerFunction CookerFunction,
    bool UsesSteamer,
    List<IngredientDto>? Ingredients,
    List<string>? Steps,
    List<string>? Tags);

public sealed record RecipeSummaryDto(
    Guid Id,
    string Title,
    string? Description,
    int Servings,
    int CookingTimeMinutes,
    CookerFunction CookerFunction,
    bool UsesSteamer,
    List<string> Tags,
    string? CoverUrl,
    bool HasVideo);

public sealed record RecipeDetailDto(
    Guid Id,
    string Title,
    string? Description,
    int Servings,
    decimal RiceCups,
    int CookingTimeMinutes,
    CookerFunction CookerFunction,
    bool UsesSteamer,
    List<IngredientDto> Ingredients,
    List<string> Steps,
    List<string> Tags,
    List<MediaDto> Media,
    DateTime CreatedAt,
    DateTime UpdatedAt);

public sealed record VideoUrlRequest(string Url, string? Caption);
