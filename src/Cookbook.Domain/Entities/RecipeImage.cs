using Cookbook.Domain.Common;

namespace Cookbook.Domain.Entities;

/// <summary>
/// Represents an image associated with a recipe.
/// </summary>
public sealed class RecipeImage : AuditableEntity
{
    public Guid RecipeId { get; private set; }

    public Recipe Recipe { get; private set; } = null!;

    /// <summary>
    /// Original file name uploaded by the user.
    /// </summary>
    public string OriginalFileName { get; private set; } = string.Empty;

    /// <summary>
    /// Internal stored file name.
    /// </summary>
    public string StoredFileName { get; private set; } = string.Empty;

    /// <summary>
    /// MIME type.
    /// </summary>
    public string ContentType { get; private set; } = string.Empty;

    /// <summary>
    /// File size in bytes.
    /// </summary>
    public long FileSize { get; private set; }

    /// <summary>
    /// Image width in pixels.
    /// </summary>
    public int Width { get; private set; }

    /// <summary>
    /// Image height in pixels.
    /// </summary>
    public int Height { get; private set; }

    /// <summary>
    /// SHA256 checksum.
    /// </summary>
    public string Checksum { get; private set; } = string.Empty;

    /// <summary>
    /// Display order.
    /// </summary>
    public int SortOrder { get; private set; }

    /// <summary>
    /// Indicates whether this is the primary image.
    /// </summary>
    public bool IsPrimary { get; private set; }

    private RecipeImage()
    {
    }

    public RecipeImage(
        Recipe recipe,
        string originalFileName,
        string storedFileName,
        string contentType,
        long fileSize,
        int width,
        int height,
        string checksum,
        bool isPrimary = false,
        int sortOrder = 0)
    {
        ArgumentNullException.ThrowIfNull(recipe);

        SetOriginalFileName(originalFileName);
        SetStoredFileName(storedFileName);
        SetContentType(contentType);
        SetChecksum(checksum);

        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(fileSize);
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(width);
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(height);
        ArgumentOutOfRangeException.ThrowIfNegative(sortOrder);

        Recipe = recipe;
        RecipeId = recipe.Id;

        FileSize = fileSize;
        Width = width;
        Height = height;

        IsPrimary = isPrimary;
        SortOrder = sortOrder;
    }

    public void SetAsPrimary()
    {
        IsPrimary = true;
    }

    public void RemovePrimary()
    {
        IsPrimary = false;
    }

    public void ChangeSortOrder(int order)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(order);

        SortOrder = order;
    }

    private void SetOriginalFileName(string fileName)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(fileName);

        OriginalFileName = fileName.Trim();
    }

    private void SetStoredFileName(string fileName)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(fileName);

        StoredFileName = fileName.Trim();
    }

    private void SetContentType(string contentType)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(contentType);

        ContentType = contentType.Trim();
    }

    private void SetChecksum(string checksum)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(checksum);

        Checksum = checksum.Trim();
    }

    public override string ToString()
        => OriginalFileName;
}