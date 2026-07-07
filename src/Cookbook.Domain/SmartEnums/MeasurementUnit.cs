using Cookbook.Domain.Common;

namespace Cookbook.Domain.SmartEnums;

/// <summary>
/// Represents a measurement unit.
/// </summary>
public sealed class MeasurementUnit : Enumeration
{
    public string Symbol { get; }

    private MeasurementUnit(
        int id,
        string name,
        string symbol)
        : base(id, name)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(symbol);

        Symbol = symbol.Trim();
    }

    public static readonly MeasurementUnit Gram =
        new(1, "Gram", "g");

    public static readonly MeasurementUnit Kilogram =
        new(2, "Kilogram", "kg");

    public static readonly MeasurementUnit Milliliter =
        new(3, "Milliliter", "ml");

    public static readonly MeasurementUnit Liter =
        new(4, "Liter", "l");

    public static readonly MeasurementUnit Teaspoon =
        new(5, "Teaspoon", "tsp");

    public static readonly MeasurementUnit Tablespoon =
        new(6, "Tablespoon", "tbsp");

    public static readonly MeasurementUnit Cup =
        new(7, "Cup", "cup");

    public static readonly MeasurementUnit Piece =
        new(8, "Piece", "pcs");

    public static readonly MeasurementUnit Slice =
        new(9, "Slice", "slice");

    public static readonly MeasurementUnit Pinch =
        new(10, "Pinch", "pinch");

    public static readonly MeasurementUnit Clove =
        new(11, "Clove", "clove");

    public static readonly MeasurementUnit Can =
        new(12, "Can", "can");

    public static readonly MeasurementUnit Bottle =
        new(13, "Bottle", "bottle");

    public static readonly MeasurementUnit Package =
        new(14, "Package", "pkg");

    public static readonly MeasurementUnit ToTaste =
        new(15, "To Taste", "q.b.");

    /// <summary>
    /// Gets all available measurement units.
    /// </summary>
    public static IReadOnlyCollection<MeasurementUnit> List =>
        GetAll<MeasurementUnit>();

    public static MeasurementUnit FromId(int id) =>
        Enumeration.FromId<MeasurementUnit>(id);

    public static MeasurementUnit FromName(string name) =>
        Enumeration.FromName<MeasurementUnit>(name);

    public override string ToString()
        => Symbol;
}