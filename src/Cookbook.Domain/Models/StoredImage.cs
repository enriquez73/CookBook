namespace Cookbook.Domain.Models;

public sealed record StoredImage(
    string BlobName,
    string OriginalFileName,
    string ContentType,
    long Length,
    string Checksum);