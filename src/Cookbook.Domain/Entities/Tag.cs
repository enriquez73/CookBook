using Cookbook.Domain.Common;

namespace Cookbook.Domain.Entities;

public sealed class Tag : AuditableEntity
{
    public string Name { get; private set; } = string.Empty;

    private Tag()
    {
    }

    public Tag(string name)
    {
        Rename(name);
    }

    public void Rename(string name)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);

        name = name.Trim();

        if (name.Length > 100)
        {
            throw new ArgumentException(
                "Tag name cannot exceed 100 characters.",
                nameof(name));
        }

        Name = name;
    }

    public override string ToString() => Name;
}