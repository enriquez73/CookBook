using Cookbook.Domain.Common;

namespace Cookbook.Domain.Entities;

/// <summary>
/// Represents an image associated with a recipe.
/// </summary>
public sealed class RecipeImage : AuditableEntity
{
    /// <summary>
    /// Gets the recipe identifier.
    /// </summary>
    public Guid RecipeId { get; private set; }

    /// <summary>
    /// Gets the parent recipe.
    /// </summary>
    public Recipe Recipe { get; private set; } = null!;

    /// <summary>
    /// Gets the original uploaded file name.
    /// </summary>
    public string OriginalFileName { get; private set; } = string.Empty;

    /// <summary>
    /// Gets the stored file name.
    /// </summary>
    public string StoredFileName { get; private set; } = string.Empty;

    /// <summary>
    /// Gets the MIME content type.
    /// </summary>
    public string ContentType { get; private set; } = string.Empty;

    /// <summary>
    /// Gets the file size in bytes.
    /// </summary>
    public long FileSize { get; private set; }

    /// <summary>
    /// Gets the image width in pixels.
    /// </summary>
    public int Width { get; private set; }

    /// <summary>
    /// Gets the image height in pixels.
    /// </summary>
    public int Height { get; private set; }

    /// <summary>
    /// Gets the SHA256 checksum.
    /// </summary>
    public string Checksum { get; private set; } = string.Empty;

    /// <summary>
    /// Gets the image display order.
    /// </summary>
    public int SortOrder { get; private set; }

    /// <summary>
    /// Gets whether this is the primary image.
    /// </summary>
    public bool IsPrimary { get; private set; }

    /// <summary>
    /// Required by Entity Framework.
    /// </summary>
    private RecipeImage()
    {
    }

    internal RecipeImage(
        Recipe recipe,
        string originalFileName,
        string storedFileName,
        string contentType,
        long fileSize,
        int width,
        int height,
        string checksum,
        bool isPrimary,
        int sortOrder)
    {
        ArgumentNullException.ThrowIfNull(recipe);

        Recipe = recipe;
        RecipeId = recipe.Id;

        SetOriginalFileName(originalFileName, 255);
        SetStoredFileName(storedFileName, 255);
        SetContentType(contentType, 100);
        SetChecksum(checksum, 128);

        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(fileSize);
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(width);
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(height);
        ArgumentOutOfRangeException.ThrowIfNegative(sortOrder);

        FileSize = fileSize;
        Width = width;
        Height = height;

        SortOrder = sortOrder;
        IsPrimary = isPrimary;
    }

    public void SetAsPrimary()
    {
        IsPrimary = true;
    }

    public void RemovePrimary()
    {
        IsPrimary = false;
    }

    internal void ChangeSortOrder(int sortOrder)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(sortOrder);

        SortOrder = sortOrder;
    }

    private void SetOriginalFileName(string fileName, int maxLength)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(fileName);

        if (fileName.Length > maxLength)
        {
            throw new ArgumentException(
                $"Original file name cannot exceed {maxLength} characters.",
                nameof(fileName));
        }

        OriginalFileName = fileName.Trim();
    }

    private void SetStoredFileName(string fileName, int maxLength)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(fileName);

        if (fileName.Length > maxLength)
        {
            throw new ArgumentException(
                $"Stored file name cannot exceed {maxLength} characters.",
                nameof(fileName));
        }

        StoredFileName = fileName.Trim();
    }

    private void SetContentType(string contentType, int maxLength)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(contentType);

        if (contentType.Length > maxLength)
        {
            throw new ArgumentException(
                $"Content type cannot exceed {maxLength} characters.",
                nameof(contentType));
        }

        ContentType = contentType.Trim();
    }

    private void SetChecksum(string checksum, int maxLength)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(checksum);
if (checksum.Length > maxLength)
        {
            throw new ArgumentException(
                $"Checksum cannot exceed {maxLength} characters.",
                nameof(checksum));
        }
        Checksum = checksum.Trim();
    }

    public override string ToString()
        => OriginalFileName;
}