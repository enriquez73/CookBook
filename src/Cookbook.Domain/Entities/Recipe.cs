namespace Cookbook.Domain.Entities;

public sealed class Recipe
{
    public const int MaxServings = 100;

    public Guid Id { get; set; } = Guid.NewGuid();
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }

    /// <summary>Persone servite.</summary>
    public int Servings { get; set; } = 2;

    /// <summary>Tazze di riso crudo; 0 se la ricetta non usa riso.</summary>
    public decimal RiceCups { get; set; }

    public int CookingTimeMinutes { get; set; }
    public List<RecipeCookingAppliance> CookingAppliances { get; set; } = [];
    /// <summary>Programma del cuociriso, nullo per gli altri tipi di attrezzatura.</summary>
    public CookerFunction? CookerFunction { get; set; } = global::Cookbook.Domain.Entities.CookerFunction.WhiteRice;
    public bool UsesSteamer { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    public List<RecipeIngredient> Ingredients { get; set; } = [];
    public List<RecipeStep> Steps { get; set; } = [];
    public List<Tag> Tags { get; set; } = [];
    public List<RecipeMedia> Media { get; set; } = [];
}

public sealed class RecipeIngredient
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid RecipeId { get; set; }
    public int Order { get; set; }
    public string Name { get; set; } = string.Empty;

    /// <summary>Quantità libera, es. "200 g" oppure "q.b.".</summary>
    public string? Quantity { get; set; }
}

public sealed class RecipeStep
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid RecipeId { get; set; }
    public int Order { get; set; }
    public string Text { get; set; } = string.Empty;
    public string? ImageFileName { get; set; }
    public string? ImageContentType { get; set; }
}

public sealed class RecipeCookingAppliance
{
    public Guid RecipeId { get; set; }
    public CookingAppliance Appliance { get; set; }
}

public sealed class Tag
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Name { get; set; } = string.Empty;
    public List<Recipe> Recipes { get; set; } = [];
}

public enum MediaKind
{
    Photo,
    VideoFile,
    VideoUrl
}

public sealed class RecipeMedia
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid RecipeId { get; set; }
    public MediaKind Kind { get; set; }
    public int Order { get; set; }
    public string? Caption { get; set; }

    /// <summary>Nome del file salvato (Photo/VideoFile).</summary>
    public string? FileName { get; set; }
    public string? ContentType { get; set; }

    /// <summary>URL esterno (VideoUrl, es. YouTube).</summary>
    public string? Url { get; set; }
}
