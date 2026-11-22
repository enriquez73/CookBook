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

    public async Task<string?> SaveImageAsync(Stream source, string contentType, long maxBytes, CancellationToken cancellationToken)
    {
        var relativeName = $"images/{Guid.NewGuid():N}{Extensions[contentType]}";
        var path = GetPath(relativeName);
        Directory.CreateDirectory(Path.GetDirectoryName(path)!);
        long total = 0;
        var tooLarge = false;
        var validImage = false;

        try
        {
            await using (var destination = File.Create(path))
            {
                var buffer = new byte[81920];
                var headerLength = 0;
                while (headerLength < 12)
                {
                    var headerRead = await source.ReadAsync(buffer.AsMemory(headerLength, 12 - headerLength), cancellationToken);
                    if (headerRead == 0) break;
                    headerLength += headerRead;
                }

                validImage = HasValidImageHeader(buffer.AsSpan(0, headerLength), contentType);
                if (validImage)
                {
                    total = headerLength;
                    await destination.WriteAsync(buffer.AsMemory(0, headerLength), cancellationToken);
                }

                int read;
                while (validImage && (read = await source.ReadAsync(buffer.AsMemory(), cancellationToken)) > 0)
                {
                    total += read;
                    if (total > maxBytes)
                    {
                        tooLarge = true;
                        break;
                    }
                    await destination.WriteAsync(buffer.AsMemory(0, read), cancellationToken);
                }
            }

            if (!validImage || tooLarge || total == 0)
            {
                File.Delete(path);
                return null;
            }

            return relativeName;
        }
        catch
        {
            if (File.Exists(path)) File.Delete(path);
            throw;
        }
    }

    public void Delete(string fileName)
    {
        var path = GetPath(fileName);
        if (File.Exists(path)) File.Delete(path);
    }

    private string GetPath(string relativeName)
    {
        var normalized = relativeName.Replace('/', Path.DirectorySeparatorChar)
            .Replace('\\', Path.DirectorySeparatorChar);
        var path = Path.GetFullPath(Path.Combine(Root, normalized));
        var rootWithSeparator = Root.EndsWith(Path.DirectorySeparatorChar)
            ? Root
            : Root + Path.DirectorySeparatorChar;
        if (!path.StartsWith(rootWithSeparator, OperatingSystem.IsWindows() ? StringComparison.OrdinalIgnoreCase : StringComparison.Ordinal))
            throw new InvalidOperationException("Il percorso del file non è valido.");
        return path;
    }

    private static bool HasValidImageHeader(ReadOnlySpan<byte> header, string contentType) => contentType switch
    {
        "image/jpeg" => header.Length >= 3 && header[..3].SequenceEqual(new byte[] { 0xFF, 0xD8, 0xFF }),
        "image/png" => header.Length >= 8 && header[..8].SequenceEqual(new byte[] { 0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A }),
        "image/webp" => header.Length >= 12
            && header[..4].SequenceEqual("RIFF"u8)
            && header.Slice(8, 4).SequenceEqual("WEBP"u8),
        "image/gif" => header.Length >= 6
            && (header[..6].SequenceEqual("GIF87a"u8) || header[..6].SequenceEqual("GIF89a"u8)),
        _ => false
    };
}
