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
    CookerFunction? CookerFunction,
    bool UsesSteamer,
    List<IngredientDto>? Ingredients,
    List<string>? Steps,
    List<string>? Tags,
    List<CookingAppliance>? CookingAppliances = null,
    CookingAppliance? CookingAppliance = null);

public sealed record RecipeSummaryDto(
    Guid Id,
    string Title,
    string? Description,
    int Servings,
    int CookingTimeMinutes,
    CookerFunction? CookerFunction,
    bool UsesSteamer,
    List<string> Tags,
    string? CoverUrl,
    bool HasVideo,
    CookingAppliance CookingAppliance,
    List<CookingAppliance> CookingAppliances);

public sealed record RecipeDetailDto(
    Guid Id,
    string Title,
    string? Description,
    int Servings,
    decimal RiceCups,
    int CookingTimeMinutes,
    CookerFunction? CookerFunction,
    bool UsesSteamer,
    List<IngredientDto> Ingredients,
    List<string> Steps,
    List<string> Tags,
    List<MediaDto> Media,
    DateTime CreatedAt,
    DateTime UpdatedAt,
    CookingAppliance CookingAppliance,
    List<CookingAppliance> CookingAppliances);

public sealed record VideoUrlRequest(string Url, string? Caption);

public sealed record ImageUrlRequest(string Url, string? Caption);
