namespace Cookbook.Api.Endpoints;

/// <summary>Salva i file caricati nel filesystem locale.</summary>
public sealed class MediaStorage
{
    public static readonly Dictionary<string, string> Extensions = new()
    {
        ["image/jpeg"] = ".jpg", ["image/png"] = ".png", ["image/webp"] = ".webp", ["image/gif"] = ".gif",
        ["video/mp4"] = ".mp4", ["video/webm"] = ".webm", ["video/quicktime"] = ".mov"
    };

    public static readonly HashSet<string> PhotoTypes = Extensions.Keys.Where(k => k.StartsWith("image/")).ToHashSet();
    public static readonly HashSet<string> VideoTypes = Extensions.Keys.Where(k => k.StartsWith("video/")).ToHashSet();

    public string Root { get; }

    public MediaStorage(string root)
    {
        Root = Path.GetFullPath(root);
        Directory.CreateDirectory(Root);
    }

    public async Task<string> SaveAsync(IFormFile file, string contentType)
    {
        var name = $"{Guid.NewGuid():N}{Extensions[contentType]}";
        await using var stream = File.Create(Path.Combine(Root, name));
        await file.CopyToAsync(stream);
        return name;
    }

    public void Delete(string fileName)
    {
        var path = Path.Combine(Root, Path.GetFileName(fileName));
        if (File.Exists(path)) File.Delete(path);
    }
}
