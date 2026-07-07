namespace Cookbook.Domain.Common;

public static class NameUtility
{
    public static string Normalize(string name)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);

        return name.Trim();
    }
}

