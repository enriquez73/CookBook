using System.Reflection;

namespace Cookbook.Domain.Common;

/// <summary>
/// Base class for strongly typed enumerations.
/// </summary>
public abstract class Enumeration :
    IComparable,
    IEquatable<Enumeration>
{
    public int Id { get; }

    public string Name { get; }

    protected Enumeration(int id, string name)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(id);
        ArgumentException.ThrowIfNullOrWhiteSpace(name);

        Id = id;
        Name = name.Trim();
    }

    public override string ToString()
        => Name;

    public override bool Equals(object? obj)
        => obj is Enumeration other &&
           GetType() == other.GetType() &&
           Id == other.Id;

    public bool Equals(Enumeration? other)
        => Equals((object?)other);

    public override int GetHashCode()
        => HashCode.Combine(GetType(), Id);

    public int CompareTo(object? other)
    {
        if (other is not Enumeration enumeration)
            throw new ArgumentException(
                $"Cannot compare {GetType().Name} with {other?.GetType().Name ?? "null"}.");

        return Id.CompareTo(enumeration.Id);
    }

    public static IReadOnlyCollection<T> GetAll<T>()
        where T : Enumeration
    {
        return typeof(T)
            .GetFields(BindingFlags.Public | BindingFlags.Static | BindingFlags.DeclaredOnly)
            .Select(f => f.GetValue(null))
            .Cast<T>()
            .OrderBy(e => e.Id)
            .ToList()
            .AsReadOnly();
    }

    public static T FromId<T>(int id)
        where T : Enumeration
    {
        return GetAll<T>()
            .Single(e => e.Id == id);
    }

    public static T FromName<T>(string name)
        where T : Enumeration
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);

        return GetAll<T>()
            .Single(e =>
                e.Name.Equals(name.Trim(),
                    StringComparison.OrdinalIgnoreCase));
    }
}