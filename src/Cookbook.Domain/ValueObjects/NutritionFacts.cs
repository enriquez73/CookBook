namespace Cookbook.Domain.ValueObjects;

/// <summary>
/// Represents the nutritional values of a food per 100 g or 100 ml.
/// </summary>
public sealed record NutritionFacts
{
    /// <summary>
    /// Energy in kilocalories (kcal).
    /// </summary>
    public decimal Calories { get; }

    /// <summary>
    /// Protein in grams.
    /// </summary>
    public decimal Protein { get; }

    /// <summary>
    /// Carbohydrates in grams.
    /// </summary>
    public decimal Carbohydrates { get; }

    /// <summary>
    /// Fat in grams.
    /// </summary>
    public decimal Fat { get; }

    /// <summary>
    /// Fiber in grams.
    /// </summary>
    public decimal Fiber { get; }

    /// <summary>
    /// Sugar in grams.
    /// </summary>
    public decimal Sugar { get; }

    /// <summary>
    /// Satured Fat in grams.
    /// </summary>
    public decimal SaturatedFat { get; }

    /// <summary>
    /// Salt in grams.
    /// </summary>
    public decimal Salt { get; }

    /// <summary>
    /// Creates a new nutritional profile.
    /// All values refer to 100 g or 100 ml of product.
    /// </summary>
    public NutritionFacts(
        decimal calories,
        decimal protein,
        decimal carbohydrates,
        decimal fat,
        decimal fiber,
        decimal sugar,
        decimal saturatedFat,
        decimal salt)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(calories);
        ArgumentOutOfRangeException.ThrowIfNegative(protein);
        ArgumentOutOfRangeException.ThrowIfNegative(carbohydrates);
        ArgumentOutOfRangeException.ThrowIfNegative(fat);
        ArgumentOutOfRangeException.ThrowIfNegative(fiber);
        ArgumentOutOfRangeException.ThrowIfNegative(sugar);
        ArgumentOutOfRangeException.ThrowIfNegative(saturatedFat);
        ArgumentOutOfRangeException.ThrowIfNegative(salt);

        Calories = calories;
        Protein = protein;
        Carbohydrates = carbohydrates;
        Fat = fat;
        Fiber = fiber;
        Sugar = sugar;
        SaturatedFat = saturatedFat;
        Salt = salt;
    }

    public override string ToString()
        => $"{Calories} kcal / 100 g";
}