using Cookbook.Domain.Entities;
using Cookbook.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Cookbook.Api.Endpoints;

public static class RecipesEndpoints
{
    private const long MaxPhotoBytes = 15L * 1024 * 1024;
    private const long MaxVideoBytes = 300L * 1024 * 1024;

    public static IEndpointRouteBuilder MapRecipesEndpoints(this IEndpointRouteBuilder app)
    {
        app.MapGet("/api/cooker-functions", () => Enum.GetNames<CookerFunction>());

        app.MapGet("/api/tags", async (CookbookDbContext db) =>
            await db.Tags.Where(t => t.Recipes.Any()).OrderBy(t => t.Name).Select(t => t.Name).ToListAsync());

        var g = app.MapGroup("/api/recipes");
        g.MapGet("/", List);
        g.MapGet("/{id:guid}", Get);
        g.MapPost("/", Create);
        g.MapPut("/{id:guid}", Update);
        g.MapDelete("/{id:guid}", Delete);
        g.MapPost("/{id:guid}/media", UploadMedia).DisableAntiforgery();
        g.MapPost("/{id:guid}/media/video-url", AddVideoUrl);
        g.MapDelete("/{id:guid}/media/{mediaId:guid}", DeleteMedia);
        return app;
    }

    private static IQueryable<Recipe> Full(CookbookDbContext db) =>
        db.Recipes.Include(r => r.Ingredients).Include(r => r.Steps).Include(r => r.Tags).Include(r => r.Media);

    private static async Task<IResult> List(
        CookbookDbContext db, string? search, CookerFunction? cookerFunction, string? tag,
        int? maxMinutes, int? servings)
    {
        var q = db.Recipes.AsNoTracking().AsQueryable();
        if (!string.IsNullOrWhiteSpace(search))
        {
            var s = search.Trim().ToLower();
            q = q.Where(r => r.Title.ToLower().Contains(s)
                || (r.Description != null && r.Description.ToLower().Contains(s))
                || r.Ingredients.Any(i => i.Name.ToLower().Contains(s))
                || r.Tags.Any(t => t.Name.ToLower().Contains(s)));
        }
        if (cookerFunction is not null) q = q.Where(r => r.CookerFunction == cookerFunction);
        if (!string.IsNullOrWhiteSpace(tag)) q = q.Where(r => r.Tags.Any(t => t.Name == tag.Trim().ToLower()));
        if (maxMinutes is not null) q = q.Where(r => r.CookingTimeMinutes <= maxMinutes);
        if (servings is not null) q = q.Where(r => r.Servings == servings);

        var recipes = await q.Include(r => r.Tags).Include(r => r.Media)
            .OrderByDescending(r => r.CreatedAt).ToListAsync();
        return Results.Ok(recipes.Select(ToSummary));
    }

    private static async Task<IResult> Get(Guid id, CookbookDbContext db)
    {
        var r = await Full(db).AsNoTracking().AsSplitQuery().FirstOrDefaultAsync(x => x.Id == id);
        return r is null ? Results.NotFound() : Results.Ok(ToDetail(r));
    }

    private static async Task<IResult> Create(RecipeRequest req, CookbookDbContext db)
    {
        if (Validate(req) is { } errors) return Results.ValidationProblem(errors);
        var r = new Recipe();
        await Apply(r, req, db);
        db.Recipes.Add(r);
        await db.SaveChangesAsync();
        return Results.Created($"/api/recipes/{r.Id}", ToDetail(r));
    }

    private static async Task<IResult> Update(Guid id, RecipeRequest req, CookbookDbContext db)
    {
        if (Validate(req) is { } errors) return Results.ValidationProblem(errors);
        var r = await Full(db).AsSplitQuery().FirstOrDefaultAsync(x => x.Id == id);
        if (r is null) return Results.NotFound();
        db.RemoveRange(r.Ingredients);
        db.RemoveRange(r.Steps);
        await Apply(r, req, db);
        db.AddRange(r.Ingredients);
        db.AddRange(r.Steps);
        await db.SaveChangesAsync();
        return Results.Ok(ToDetail(r));
    }

    private static async Task<IResult> Delete(Guid id, CookbookDbContext db, MediaStorage storage)
    {
        var r = await db.Recipes.Include(x => x.Media).FirstOrDefaultAsync(x => x.Id == id);
        if (r is null) return Results.NotFound();
        db.Recipes.Remove(r);
        await db.SaveChangesAsync();
        foreach (var m in r.Media.Where(m => m.FileName is not null)) storage.Delete(m.FileName!);
        return Results.NoContent();
    }

    private static async Task<IResult> UploadMedia(
        Guid id, IFormFile file, string? caption, CookbookDbContext db, MediaStorage storage)
    {
        var r = await db.Recipes.Include(x => x.Media).FirstOrDefaultAsync(x => x.Id == id);
        if (r is null) return Results.NotFound();

        var type = file.ContentType.ToLowerInvariant();
        MediaKind kind;
        if (MediaStorage.PhotoTypes.Contains(type)) kind = MediaKind.Photo;
        else if (MediaStorage.VideoTypes.Contains(type)) kind = MediaKind.VideoFile;
        else return Invalid("file", "Formato non supportato (foto: JPEG, PNG, WebP, GIF; video: MP4, WebM, MOV).");

        if (file.Length == 0 || file.Length > (kind == MediaKind.Photo ? MaxPhotoBytes : MaxVideoBytes))
            return Invalid("file", "File vuoto o troppo grande.");

        var stored = await storage.SaveAsync(file, type);
        var media = new RecipeMedia
        {
            Kind = kind, FileName = stored, ContentType = type,
            Caption = string.IsNullOrWhiteSpace(caption) ? null : caption.Trim(),
            Order = r.Media.Count
        };
        media.RecipeId = id;
        r.Media.Add(media);
        db.Add(media);
        r.UpdatedAt = DateTime.UtcNow;
        await db.SaveChangesAsync();
        return Results.Created($"/api/recipes/{id}", ToMedia(media));
    }

    private static async Task<IResult> AddVideoUrl(Guid id, VideoUrlRequest req, CookbookDbContext db)
    {
        if (!Uri.TryCreate(req.Url, UriKind.Absolute, out var uri) || uri.Scheme is not ("http" or "https"))
            return Invalid("url", "URL non valido.");
        var r = await db.Recipes.Include(x => x.Media).FirstOrDefaultAsync(x => x.Id == id);
        if (r is null) return Results.NotFound();
        var media = new RecipeMedia
        {
            Kind = MediaKind.VideoUrl, Url = uri.ToString(), Order = r.Media.Count,
            Caption = string.IsNullOrWhiteSpace(req.Caption) ? null : req.Caption.Trim()
        };
        media.RecipeId = id;
        r.Media.Add(media);
        db.Add(media);
        r.UpdatedAt = DateTime.UtcNow;
        await db.SaveChangesAsync();
        return Results.Created($"/api/recipes/{id}", ToMedia(media));
    }

    private static async Task<IResult> DeleteMedia(Guid id, Guid mediaId, CookbookDbContext db, MediaStorage storage)
    {
        var m = await db.Set<RecipeMedia>().FirstOrDefaultAsync(x => x.Id == mediaId && x.RecipeId == id);
        if (m is null) return Results.NotFound();
        db.Remove(m);
        await db.SaveChangesAsync();
        if (m.FileName is not null) storage.Delete(m.FileName);
        return Results.NoContent();
    }

    private static IResult Invalid(string field, string message) =>
        Results.ValidationProblem(new Dictionary<string, string[]> { [field] = [message] });

    private static Dictionary<string, string[]>? Validate(RecipeRequest r)
    {
        var e = new Dictionary<string, string[]>();
        if (string.IsNullOrWhiteSpace(r.Title)) e["title"] = ["Il titolo è obbligatorio."];
        else if (r.Title.Length > 200) e["title"] = ["Il titolo è troppo lungo (max 200)."];
        if (r.Servings is < 1 or > Recipe.MaxServings) e["servings"] = ["Le porzioni devono essere da 1 a 4."];
        if (r.RiceCups is < 0 or > 4) e["riceCups"] = ["Le tazze di riso crudo devono essere da 0 a 4."];
        if (r.CookingTimeMinutes is < 0 or > 1440) e["cookingTimeMinutes"] = ["Tempo di cottura non valido."];
        if (!Enum.IsDefined(r.CookerFunction)) e["cookerFunction"] = ["Funzione non valida."];
        if (r.Ingredients?.Any(i => string.IsNullOrWhiteSpace(i.Name)) == true) e["ingredients"] = ["Ogni ingrediente deve avere un nome."];
        return e.Count > 0 ? e : null;
    }

    private static async Task Apply(Recipe r, RecipeRequest req, CookbookDbContext db)
    {
        r.Title = req.Title.Trim();
        r.Description = string.IsNullOrWhiteSpace(req.Description) ? null : req.Description.Trim();
        r.Servings = req.Servings;
        r.RiceCups = req.RiceCups;
        r.CookingTimeMinutes = req.CookingTimeMinutes;
        r.CookerFunction = req.CookerFunction;
        r.UsesSteamer = req.UsesSteamer;
        r.UpdatedAt = DateTime.UtcNow;

        r.Ingredients = (req.Ingredients ?? [])
            .Select((i, n) => new RecipeIngredient
            {
                Order = n, Name = i.Name.Trim(),
                Quantity = string.IsNullOrWhiteSpace(i.Quantity) ? null : i.Quantity.Trim()
            }).ToList();
        r.Steps = (req.Steps ?? []).Where(s => !string.IsNullOrWhiteSpace(s))
            .Select((s, n) => new RecipeStep { Order = n, Text = s.Trim() }).ToList();

        var names = (req.Tags ?? []).Select(t => t.Trim().ToLowerInvariant())
            .Where(t => t.Length > 0).Select(t => t.Length > 50 ? t[..50] : t).Distinct().ToList();
        var existing = await db.Tags.Where(t => names.Contains(t.Name)).ToListAsync();
        r.Tags = names.Select(n =>
        {
            var tag = existing.FirstOrDefault(t => t.Name == n);
            if (tag is null) db.Tags.Add(tag = new Tag { Name = n });
            return tag;
        }).ToList();
    }

    private static string MediaUrl(RecipeMedia m) => m.Kind == MediaKind.VideoUrl ? m.Url! : $"/media/{m.FileName}";

    private static MediaDto ToMedia(RecipeMedia m) => new(m.Id, m.Kind, MediaUrl(m), m.Caption);

    private static RecipeSummaryDto ToSummary(Recipe r) => new(
        r.Id, r.Title, r.Description, r.Servings, r.CookingTimeMinutes, r.CookerFunction, r.UsesSteamer,
        r.Tags.Select(t => t.Name).Order().ToList(),
        r.Media.Where(m => m.Kind == MediaKind.Photo).OrderBy(m => m.Order).Select(MediaUrl).FirstOrDefault(),
        r.Media.Any(m => m.Kind != MediaKind.Photo));

    private static RecipeDetailDto ToDetail(Recipe r) => new(
        r.Id, r.Title, r.Description, r.Servings, r.RiceCups, r.CookingTimeMinutes, r.CookerFunction, r.UsesSteamer,
        r.Ingredients.OrderBy(i => i.Order).Select(i => new IngredientDto(i.Name, i.Quantity)).ToList(),
        r.Steps.OrderBy(s => s.Order).Select(s => s.Text).ToList(),
        r.Tags.Select(t => t.Name).Order().ToList(),
        r.Media.OrderBy(m => m.Order).Select(ToMedia).ToList(),
        r.CreatedAt, r.UpdatedAt);
}
