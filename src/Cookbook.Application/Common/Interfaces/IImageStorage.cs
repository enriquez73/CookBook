using Cookbook.Domain.Models;

namespace Cookbook.Application.Common.Interfaces;

public interface IImageStorage
{
    Task<StoredImage> SaveAsync(
        Stream stream,
        string fileName,
        string contentType,
        CancellationToken cancellationToken = default);

    Task<Stream> OpenReadAsync(
        string blobName,
        CancellationToken cancellationToken = default);

    Task DeleteAsync(
        string blobName,
        CancellationToken cancellationToken = default);

    Task<bool> ExistsAsync(
        string blobName,
        CancellationToken cancellationToken = default);
}