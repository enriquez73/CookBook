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
        app.MapGet("/api/cooking-appliances", () => Enum.GetNames<CookingAppliance>());

        app.MapGet("/api/tags", async (CookbookDbContext db) =>
            await db.Tags.Where(t => t.Recipes.Any()).OrderBy(t => t.Name).Select(t => t.Name).ToListAsync());

        var g = app.MapGroup("/api/recipes");
        g.MapGet("/", List);
        g.MapGet("/{id:guid}", Get);
        g.MapPost("/", Create);
        g.MapPut("/{id:guid}", Update);
        g.MapDelete("/{id:guid}", Delete);
        g.MapPost("/{id:guid}/media", UploadMedia).DisableAntiforgery();
        g.MapPost("/{id:guid}/media/image-url", AddImageUrl);
        g.MapPost("/{id:guid}/media/video-url", AddVideoUrl);
        g.MapDelete("/{id:guid}/media/{mediaId:guid}", DeleteMedia);
        g.MapPost("/{id:guid}/steps/{stepId:guid}/image", UploadStepImage)
            .DisableAntiforgery()
            .WithName("UploadRecipeStepImage")
            .WithSummary("Carica o sostituisce l'immagine di un passaggio")
            .Produces<RecipeStepDto>()
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status404NotFound);
        g.MapPost("/{id:guid}/steps/{stepId:guid}/image-url", AddStepImageUrl)
            .WithName("AddRecipeStepImageFromUrl")
            .WithSummary("Scarica un'immagine da URL e la assegna a un passaggio")
            .Produces<RecipeStepDto>()
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status404NotFound);
        g.MapDelete("/{id:guid}/steps/{stepId:guid}/image", DeleteStepImage)
            .WithName("DeleteRecipeStepImage")
            .WithSummary("Rimuove l'immagine di un passaggio")
            .Produces(StatusCodes.Status204NoContent)
            .Produces(StatusCodes.Status404NotFound);
        return app;
    }

    private static IQueryable<Recipe> Full(CookbookDbContext db) =>
        db.Recipes.Include(r => r.Ingredients).Include(r => r.Steps).Include(r => r.Tags).Include(r => r.Media)
            .Include(r => r.CookingAppliances);

    private static async Task<IResult> List(
        CookbookDbContext db, string? search, CookingAppliance? cookingAppliance, CookerFunction? cookerFunction, string? tag,
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
        if (cookingAppliance is not null) q = q.Where(r => r.CookingAppliances.Any(a => a.Appliance == cookingAppliance));
        if (cookerFunction is not null) q = q.Where(r => r.CookerFunction == cookerFunction);
        if (!string.IsNullOrWhiteSpace(tag)) q = q.Where(r => r.Tags.Any(t => t.Name == tag.Trim().ToLower()));
        if (maxMinutes is not null) q = q.Where(r => r.CookingTimeMinutes <= maxMinutes);
        if (servings is not null) q = q.Where(r => r.Servings == servings);

        var recipes = await q.Include(r => r.Tags).Include(r => r.Media).Include(r => r.CookingAppliances)
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
        if (req.StepIds?.Any(stepId => stepId is not null) == true)
            return Invalid("stepIds", "Una nuova ricetta non può riferirsi a passaggi già esistenti.");
        var r = new Recipe();
        await Apply(r, req, db);
        db.Recipes.Add(r);
        await db.SaveChangesAsync();
        return Results.Created($"/api/recipes/{r.Id}", ToDetail(r));
    }

    private static async Task<IResult> Update(Guid id, RecipeRequest req, CookbookDbContext db, MediaStorage storage)
    {
        if (Validate(req) is { } errors) return Results.ValidationProblem(errors);
        var r = await Full(db).AsSplitQuery().FirstOrDefaultAsync(x => x.Id == id);
        if (r is null) return Results.NotFound();
        if (req.StepIds?.Where(stepId => stepId is not null)
            .Any(stepId => r.Steps.All(step => step.Id != stepId!.Value)) == true)
            return Invalid("stepIds", "Uno dei passaggi indicati non appartiene a questa ricetta.");
        var previousStepFiles = r.Steps.ToDictionary(step => step.Id, step => step.ImageFileName);
        db.RemoveRange(r.Ingredients);
        db.RemoveRange(r.CookingAppliances);
        await Apply(r, req, db);
        var retainedStepIds = r.Steps.Select(step => step.Id).ToHashSet();
        var removedStepFiles = previousStepFiles
            .Where(entry => !retainedStepIds.Contains(entry.Key) && entry.Value is not null)
            .Select(entry => entry.Value!)
            .ToList();
        db.AddRange(r.Ingredients);
        db.AddRange(r.CookingAppliances);
        await db.SaveChangesAsync();
        foreach (var fileName in removedStepFiles) storage.Delete(fileName);
        return Results.Ok(ToDetail(r));
    }

    private static async Task<IResult> Delete(Guid id, CookbookDbContext db, MediaStorage storage)
    {
        var r = await db.Recipes.Include(x => x.Media).Include(x => x.Steps).FirstOrDefaultAsync(x => x.Id == id);
        if (r is null) return Results.NotFound();
        db.Recipes.Remove(r);
        await db.SaveChangesAsync();
        foreach (var m in r.Media.Where(m => m.FileName is not null)) storage.Delete(m.FileName!);
        foreach (var step in r.Steps.Where(step => step.ImageFileName is not null)) storage.Delete(step.ImageFileName!);
        return Results.NoContent();
    }

    private static async Task<IResult> UploadStepImage(
        Guid id, Guid stepId, IFormFile file, CookbookDbContext db, MediaStorage storage,
        CancellationToken cancellationToken)
    {
        var step = await db.Set<RecipeStep>().FirstOrDefaultAsync(
            x => x.Id == stepId && x.RecipeId == id, cancellationToken);
        if (step is null) return Results.NotFound();

        var type = file.ContentType.ToLowerInvariant();
        if (!MediaStorage.PhotoTypes.Contains(type))
            return Invalid("file", "Formato non supportato (JPEG, PNG, WebP o GIF).");
        if (file.Length == 0 || file.Length > MaxPhotoBytes)
            return Invalid("file", "L'immagine è vuota o supera 15 MB.");

        await using var source = file.OpenReadStream();
        var stored = await storage.SaveImageAsync(source, type, MaxPhotoBytes, cancellationToken);
        if (stored is null) return Invalid("file", "Il file non contiene un'immagine valida o supera 15 MB.");

        var previousFile = step.ImageFileName;
        step.ImageFileName = stored;
        step.ImageContentType = type;
        var recipe = await db.Recipes.FirstAsync(x => x.Id == id, cancellationToken);
        recipe.UpdatedAt = DateTime.UtcNow;
        try
        {
            await db.SaveChangesAsync(cancellationToken);
        }
        catch
        {
            storage.Delete(stored);
            throw;
        }
        if (previousFile is not null) storage.Delete(previousFile);

        return Results.Ok(ToStep(step));
    }

    private static async Task<IResult> AddStepImageUrl(
        Guid id, Guid stepId, ImageUrlRequest req, CookbookDbContext db, MediaStorage storage,
        RemoteImageDownloader downloader, CancellationToken cancellationToken)
    {
        var step = await db.Set<RecipeStep>().FirstOrDefaultAsync(
            x => x.Id == stepId && x.RecipeId == id, cancellationToken);
        if (step is null) return Results.NotFound();

        var downloaded = await downloader.DownloadAsync(req.Url, cancellationToken);
        if (!downloaded.IsSuccess)
            return Invalid("url", downloaded.Error ?? "Non è stato possibile scaricare l'immagine.");

        var previousFile = step.ImageFileName;
        step.ImageFileName = downloaded.FileName;
        step.ImageContentType = downloaded.ContentType;
        var recipe = await db.Recipes.FirstAsync(x => x.Id == id, cancellationToken);
        recipe.UpdatedAt = DateTime.UtcNow;
        try
        {
            await db.SaveChangesAsync(cancellationToken);
        }
        catch
        {
            storage.Delete(downloaded.FileName!);
            throw;
        }
        if (previousFile is not null) storage.Delete(previousFile);

        return Results.Ok(ToStep(step));
    }

    private static async Task<IResult> DeleteStepImage(
        Guid id, Guid stepId, CookbookDbContext db, MediaStorage storage, CancellationToken cancellationToken)
    {
        var step = await db.Set<RecipeStep>().FirstOrDefaultAsync(
            x => x.Id == stepId && x.RecipeId == id, cancellationToken);
        if (step is null) return Results.NotFound();
        if (step.ImageFileName is null) return Results.NoContent();

        var previousFile = step.ImageFileName;
        step.ImageFileName = null;
        step.ImageContentType = null;
        var recipe = await db.Recipes.FirstAsync(x => x.Id == id, cancellationToken);
        recipe.UpdatedAt = DateTime.UtcNow;
        await db.SaveChangesAsync(cancellationToken);
        storage.Delete(previousFile);
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

    private static async Task<IResult> AddImageUrl(
        Guid id, ImageUrlRequest req, CookbookDbContext db, MediaStorage storage,
        RemoteImageDownloader downloader, CancellationToken cancellationToken)
    {
        var recipe = await db.Recipes.Include(x => x.Media).FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
        if (recipe is null) return Results.NotFound();

        var downloaded = await downloader.DownloadAsync(req.Url, cancellationToken);
        if (!downloaded.IsSuccess)
            return Invalid("imageUrl", downloaded.Error ?? "Non è stato possibile scaricare l'immagine.");

        var media = new RecipeMedia
        {
            Kind = MediaKind.Photo,
            FileName = downloaded.FileName,
            ContentType = downloaded.ContentType,
            Caption = string.IsNullOrWhiteSpace(req.Caption) ? null : req.Caption.Trim(),
            Order = recipe.Media.Count
        };
        media.RecipeId = id;
        recipe.Media.Add(media);
        db.Add(media);
        recipe.UpdatedAt = DateTime.UtcNow;
        try
        {
            await db.SaveChangesAsync(cancellationToken);
        }
        catch
        {
            storage.Delete(downloaded.FileName!);
            throw;
        }

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
        if (r.Servings is < 1 or > Recipe.MaxServings) e["servings"] = ["Le porzioni devono essere da 1 a 100."];
        if (r.RiceCups is < 0 or > 100) e["riceCups"] = ["Le tazze di riso devono essere da 0 a 100."];
        if (r.CookingTimeMinutes is < 0 or > 1440) e["cookingTimeMinutes"] = ["Tempo di cottura non valido."];
        if (r.StepIds is not null && (r.Steps is null || r.StepIds.Count != r.Steps.Count))
            e["stepIds"] = ["Gli identificativi devono corrispondere ai passaggi inviati."];
        else if (r.StepIds?.Where(id => id is not null).Select(id => id!.Value).Distinct().Count() != r.StepIds?.Count(id => id is not null))
            e["stepIds"] = ["Uno stesso passaggio non può essere inviato più volte."];
        var appliances = RequestedAppliances(r);
        if (appliances.Count == 0) e["cookingAppliances"] = ["Seleziona almeno un'attrezzatura."];
        if (appliances.Any(a => !Enum.IsDefined(a))) e["cookingAppliances"] = ["Attrezzatura non valida."];
        if (appliances.Contains(CookingAppliance.RiceCooker) && (r.CookerFunction is null || !Enum.IsDefined(r.CookerFunction.Value)))
            e["cookerFunction"] = ["Seleziona un programma valido per il cuociriso."];
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
        var appliances = RequestedAppliances(req).Distinct().ToList();
        r.CookingAppliances = appliances.Select(a => new RecipeCookingAppliance { RecipeId = r.Id, Appliance = a }).ToList();
        r.CookerFunction = appliances.Contains(CookingAppliance.RiceCooker) ? req.CookerFunction : null;
        r.UsesSteamer = req.UsesSteamer;
        r.UpdatedAt = DateTime.UtcNow;

        r.Ingredients = (req.Ingredients ?? [])
            .Select((i, n) => new RecipeIngredient
            {
                Order = n, Name = i.Name.Trim(),
                Quantity = string.IsNullOrWhiteSpace(i.Quantity) ? null : i.Quantity.Trim()
            }).ToList();
        var existingSteps = r.Steps.ToDictionary(step => step.Id);
        var retainedSteps = new HashSet<Guid>();
        var steps = new List<RecipeStep>();
        foreach (var (text, index) in (req.Steps ?? []).Select((text, index) => (text, index)))
        {
            if (string.IsNullOrWhiteSpace(text)) continue;
            RecipeStep step;
            var stepId = req.StepIds is null
                ? (index < r.Steps.Count ? r.Steps[index].Id : (Guid?)null)
                : req.StepIds[index];
            if (stepId is { } existingId)
            {
                if (!existingSteps.TryGetValue(existingId, out var existingStep))
                    throw new InvalidOperationException("Il passaggio indicato non appartiene alla ricetta.");
                step = existingStep;
                retainedSteps.Add(existingId);
            }
            else
            {
                step = new RecipeStep { RecipeId = r.Id };
            }
            step.Order = steps.Count;
            step.Text = text.Trim();
            steps.Add(step);
        }
        db.RemoveRange(r.Steps.Where(step => !retainedSteps.Contains(step.Id)));
        r.Steps = steps;

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

    private static string MediaUrl(RecipeMedia m) => m.Kind == MediaKind.VideoUrl
        ? m.Url!
        : $"/media/{string.Join('/', m.FileName!.Split('/').Select(Uri.EscapeDataString))}";

    private static string? StepImageUrl(RecipeStep step) => step.ImageFileName is null
        ? null
        : $"/media/{string.Join('/', step.ImageFileName.Split('/').Select(Uri.EscapeDataString))}";

    private static List<CookingAppliance> RequestedAppliances(RecipeRequest req) =>
        req.CookingAppliances is not null
            ? req.CookingAppliances
            : [req.CookingAppliance ?? CookingAppliance.RiceCooker];

    private static MediaDto ToMedia(RecipeMedia m) => new(m.Id, m.Kind, MediaUrl(m), m.Caption);

    private static RecipeStepDto ToStep(RecipeStep step) => new(step.Id, step.Text, StepImageUrl(step));

    private static RecipeSummaryDto ToSummary(Recipe r) => new(
        r.Id, r.Title, r.Description, r.Servings, r.CookingTimeMinutes, r.CookerFunction, r.UsesSteamer,
        r.Tags.Select(t => t.Name).Order().ToList(),
        r.Media.Where(m => m.Kind == MediaKind.Photo).OrderBy(m => m.Order).Select(MediaUrl).FirstOrDefault(),
        r.Media.Any(m => m.Kind != MediaKind.Photo), PrimaryAppliance(r), Appliances(r));

    private static RecipeDetailDto ToDetail(Recipe r) => new(
        r.Id, r.Title, r.Description, r.Servings, r.RiceCups, r.CookingTimeMinutes, r.CookerFunction, r.UsesSteamer,
        r.Ingredients.OrderBy(i => i.Order).Select(i => new IngredientDto(i.Name, i.Quantity)).ToList(),
        r.Steps.OrderBy(s => s.Order).Select(s => s.Text).ToList(),
        r.Steps.OrderBy(s => s.Order).Select(ToStep).ToList(),
        r.Tags.Select(t => t.Name).Order().ToList(),
        r.Media.OrderBy(m => m.Order).Select(ToMedia).ToList(),
        r.CreatedAt, r.UpdatedAt, PrimaryAppliance(r), Appliances(r));

    private static List<CookingAppliance> Appliances(Recipe r) =>
        r.CookingAppliances.Select(a => a.Appliance).Order().ToList();

    private static CookingAppliance PrimaryAppliance(Recipe r) =>
        Appliances(r).FirstOrDefault(CookingAppliance.RiceCooker);
}
