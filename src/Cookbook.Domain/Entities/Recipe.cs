using Cookbook.Domain.Common;
using Cookbook.Domain.Enums;
using Cookbook.Domain.SmartEnums;

namespace Cookbook.Domain.Entities;

/// <summary>
/// Represents a recipe.
/// Aggregate Root of the Cookbook domain.
/// </summary>
public sealed partial class Recipe : AuditableEntity, IAggregateRoot
{
    private readonly List<RecipeIngredient> _ingredients = [];
    private readonly List<RecipeStep> _steps = [];
    private readonly List<RecipeImage> _images = [];

    private readonly List<Category> _categories = [];
    private readonly List<Tag> _tags = [];
    private readonly List<Equipment> _equipments = [];
    
    public bool HasIngredients =>
        _ingredients.Count > 0;

    public bool HasSteps =>
        _steps.Count > 0;

    public bool HasCategories =>
        _categories.Count > 0;

    public bool HasTags =>
        _tags.Count > 0;

    public bool HasEquipments =>
        _equipments.Count > 0;

    /// <summary>
    /// Gets the recipe name.
    /// </summary>
    public string Name { get; private set; } = string.Empty;

    /// <summary>
    /// Gets the recipe description.
    /// </summary>
    public string? Description { get; private set; }

    /// <summary>
    /// Gets optional notes.
    /// </summary>
    public string? Notes { get; private set; }

    /// <summary>
    /// Gets the recipe source.
    /// </summary>
    public string? Source { get; private set; }

    /// <summary>
    /// Gets the recipe difficulty.
    /// </summary>
    public Difficulty Difficulty { get; private set; }

    /// <summary>
    /// Gets the cooking program.
    /// </summary>
    public CookingProgram CookingProgram { get; private set; }

    /// <summary>
    /// Gets the preparation time in minutes.
    /// </summary>
    public int PreparationTimeMinutes { get; private set; }

    /// <summary>
    /// Gets the cooking time in minutes.
    /// </summary>
    public int CookingTimeMinutes { get; private set; }

    /// <summary>
    /// Gets the total time in minutes.
    /// </summary>
    public int TotalTimeMinutes =>
        PreparationTimeMinutes + CookingTimeMinutes;

    /// <summary>
    /// Gets the number of servings.
    /// </summary>
    public int Servings { get; private set; }

    /// <summary>
    /// Gets the ingredients.
    /// </summary>
    public IReadOnlyList<RecipeIngredient> Ingredients =>
        _ingredients.AsReadOnly();

    /// <summary>
    /// Gets the preparation steps.
    /// </summary>
    public IReadOnlyList<RecipeStep> Steps =>
        _steps.AsReadOnly();

    /// <summary>
    /// Gets the recipe images.
    /// </summary>
    public IReadOnlyList<RecipeImage> Images =>
        _images.AsReadOnly();

    /// <summary>
    /// Gets the assigned categories.
    /// </summary>
    public IReadOnlyList<Category> Categories =>
        _categories.AsReadOnly();

    /// <summary>
    /// Gets the assigned tags.
    /// </summary>
    public IReadOnlyList<Tag> Tags =>
        _tags.AsReadOnly();

    /// <summary>
    /// Gets the required equipments.
    /// </summary>
    public IReadOnlyList<Equipment> Equipments =>
        _equipments.AsReadOnly();

    /// <summary>
    /// Gets the primary image.
    /// </summary>
    public RecipeImage? PrimaryImage =>
        _images.FirstOrDefault(x => x.IsPrimary);

    /// <summary>
    /// Gets whether the recipe has images.
    /// </summary>
    public bool HasImages =>
        _images.Count > 0;

    /// <summary>
    /// Required by Entity Framework.
    /// </summary>
    private Recipe()
    {
    }

    /// <summary>
    /// Creates a new recipe.
    /// </summary>
    public Recipe(
        string name,
        Difficulty difficulty,
        CookingProgram cookingProgram,
        int preparationTimeMinutes,
        int cookingTimeMinutes,
        int servings,
        string? description = null,
        string? notes = null,
        string? source = null)
    {
        Rename(name);

        ChangeDescription(description);
        ChangeNotes(notes);
        ChangeSource(source);

        ChangeDifficulty(difficulty);
        ChangeCookingProgram(cookingProgram);

        ChangePreparationTime(preparationTimeMinutes);
        ChangeCookingTime(cookingTimeMinutes);

        ChangeServings(servings);
    }
    
    #region Recipe
    public void Update(
        string name,
        string? description,
        Difficulty difficulty,
        CookingProgram cookingProgram,
        int preparationTimeMinutes,
        int cookingTimeMinutes,
        int servings,
        string? notes,
        string? source)
    {
        Rename(name);
        ChangeDescription(description);
        ChangeDifficulty(difficulty);
        ChangeCookingProgram(cookingProgram);
        ChangePreparationTime(preparationTimeMinutes);
        ChangeCookingTime(cookingTimeMinutes);
        ChangeServings(servings);
        ChangeNotes(notes);
        ChangeSource(source);
    }
    #endregion

    #region Basic Information

    public void Rename(string name)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);

        name = name.Trim();

        if (name.Length > 200)
        {
            throw new ArgumentException(
                "Recipe name cannot exceed 200 characters.",
                nameof(name));
        }

        Name = name;
    }

    public void ChangeDescription(string? description)
    {
        Description = string.IsNullOrWhiteSpace(description)
            ? null
            : description.Trim();
    }

    public void ChangeNotes(string? notes)
    {
        Notes = string.IsNullOrWhiteSpace(notes)
            ? null
            : notes.Trim();
    }

    public void ChangeSource(string? source)
    {
        Source = string.IsNullOrWhiteSpace(source)
            ? null
            : source.Trim();
    }

    public void ChangeDifficulty(Difficulty difficulty)
    {
        Difficulty = difficulty;
    }

    public void ChangeCookingProgram(CookingProgram cookingProgram)
    {
        CookingProgram = cookingProgram;
    }

    public void ChangePreparationTime(int minutes)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(minutes);

        PreparationTimeMinutes = minutes;
    }

    public void ChangeCookingTime(int minutes)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(minutes);

        CookingTimeMinutes = minutes;
    }

    public void ChangeServings(int servings)
    {
        ArgumentOutOfRangeException.ThrowIfLessThan(servings, 1);

        Servings = servings;
    }

    #endregion

    #region Ingredients

    public RecipeIngredient AddIngredient(
        Ingredient ingredient,
        decimal quantity,
        MeasurementUnit unit,
        string? notes = null)
    {
        ArgumentNullException.ThrowIfNull(ingredient);
        ArgumentNullException.ThrowIfNull(unit);

        if (_ingredients.Any(i => i.IngredientId == ingredient.Id))
        {
            throw new InvalidOperationException(
                $"Ingredient '{ingredient.Name}' has already been added.");
        }

        var recipeIngredient = new RecipeIngredient(
            this,
            ingredient,
            quantity,
            unit,
            notes);

        _ingredients.Add(recipeIngredient);

        // ingredient.AddRecipeIngredient(recipeIngredient);

        return recipeIngredient;
    }

    public void RemoveIngredient(Guid ingredientId)
    {
        var recipeIngredient = _ingredients
            .FirstOrDefault(i => i.IngredientId == ingredientId);

        if (recipeIngredient is null)
            return;

        _ingredients.Remove(recipeIngredient);

        // recipeIngredient.Ingredient.RemoveRecipeIngredient(recipeIngredient);
    }

    #endregion

    #region Categories

    public void AddCategory(Category category)
    {
        ArgumentNullException.ThrowIfNull(category);

        if (_categories.Any(c => c.Id == category.Id))
            return;

        _categories.Add(category);
    }

    public void RemoveCategory(Guid categoryId)
    {
        var category = _categories
            .FirstOrDefault(c => c.Id == categoryId);

        if (category is not null)
        {
            _categories.Remove(category);
        }
    }

    #endregion

    #region Tags

    public void AddTag(Tag tag)
    {
        ArgumentNullException.ThrowIfNull(tag);

        if (_tags.Any(t => t.Id == tag.Id))
            return;

        _tags.Add(tag);
    }

    public void RemoveTag(Guid tagId)
    {
        var tag = _tags
            .FirstOrDefault(t => t.Id == tagId);

        if (tag is not null)
        {
            _tags.Remove(tag);
        }
    }

    #endregion

    #region Equipments

    public void AddEquipment(Equipment equipment)
    {
        ArgumentNullException.ThrowIfNull(equipment);

        if (_equipments.Any(e => e.Id == equipment.Id))
            return;

        _equipments.Add(equipment);
    }

    public void RemoveEquipment(Guid equipmentId)
    {
        var equipment = _equipments
            .FirstOrDefault(e => e.Id == equipmentId);

        if (equipment is not null)
        {
            _equipments.Remove(equipment);
        }
    }

    #endregion

    #region Steps

    public RecipeStep AddStep(string description)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(description);

        var step = new RecipeStep(
            this,
            _steps.Count + 1,
            description);

        _steps.Add(step);

        return step;
    }

    public void RemoveStep(int stepNumber)
    {
        ArgumentOutOfRangeException.ThrowIfLessThan(stepNumber, 1);

        var step = _steps.FirstOrDefault(x => x.StepNumber == stepNumber);

        if (step is null)
            return;

        _steps.Remove(step);

        ReorderSteps();
    }

    public void MoveStep(int from, int to)
    {
        ArgumentOutOfRangeException.ThrowIfLessThan(from, 1);
        ArgumentOutOfRangeException.ThrowIfLessThan(to, 1);

        if (from == to)
            return;

        if (from > _steps.Count)
            throw new ArgumentOutOfRangeException(nameof(from));

        if (to > _steps.Count)
            throw new ArgumentOutOfRangeException(nameof(to));

        var step = _steps.Single(x => x.StepNumber == from);

        _steps.Remove(step);

        _steps.Insert(to - 1, step);

        ReorderSteps();
    }

    private void ReorderSteps()
    {
        for (var i = 0; i < _steps.Count; i++)
        {
            _steps[i].ChangeStepNumber(i + 1);
        }
    }

    #endregion

    #region Images

    public RecipeImage AddImage(
        string originalFileName,
        string storedFileName,
        string contentType,
        long fileSize,
        int width,
        int height,
        string checksum,
        bool isPrimary = false)
    {
        if (_images.Count == 0)
        {
            isPrimary = true;
        }

        if (isPrimary)
        {
            foreach (var img in _images)
            {
                img.RemovePrimary();
            }
        }

        var image = new RecipeImage(
            this,
            originalFileName,
            storedFileName,
            contentType,
            fileSize,
            width,
            height,
            checksum,
            isPrimary,
            _images.Count);

        _images.Add(image);

        return image;
    }

    public void RemoveImage(Guid imageId)
    {
        var image = _images.FirstOrDefault(x => x.Id == imageId);

        if (image is null)
            return;

        var wasPrimary = image.IsPrimary;

        _images.Remove(image);

        ReorderImages();

        if (wasPrimary && _images.Count > 0)
        {
            _images[0].SetAsPrimary();
        }
    }

    public void SetPrimaryImage(Guid imageId)
    {
        var image = _images.FirstOrDefault(x => x.Id == imageId);

        if (image is null)
        {
            throw new InvalidOperationException(
                "Image not found.");
        }

        foreach (var item in _images)
        {
            item.RemovePrimary();
        }

        image.SetAsPrimary();
    }

    private void ReorderImages()
    {
        for (var i = 0; i < _images.Count; i++)
        {
            _images[i].ChangeSortOrder(i);
        }
    }

    #endregion

    public override string ToString()
    {
        return Name;
    }
}