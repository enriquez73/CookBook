namespace Cookbook.Infrastructure.Storage;

public sealed class StorageOptions
{
    public const string SectionName = "Storage";

    public string ConnectionString { get; init; } = string.Empty;

    public string ContainerName { get; init; } = "recipes";

    public bool CreateContainerIfNotExists { get; init; } = true;
}