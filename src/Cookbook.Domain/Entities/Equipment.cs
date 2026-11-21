using Cookbook.Domain.Common;

namespace Cookbook.Domain.Entities;

public sealed class Equipment : AuditableEntity
{
    public string Name { get; private set; } = string.Empty;

    private Equipment()
    {
    }

    public Equipment(string name)
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
                "Equipment name cannot exceed 100 characters.",
                nameof(name));
        }

        Name = name;
    }

    public override string ToString() => Name;
}