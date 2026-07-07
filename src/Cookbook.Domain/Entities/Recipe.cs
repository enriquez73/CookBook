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
    private readonly List<Category> _categories = [];
    private readonly List<Tag> _tags = [];
    private readonly List<Equipment> _equipments = [];
    private readonly List<RecipeImage> _images = [];

    public string Name { get; private set; } = string.Empty;

    public string? Description { get; private set; }

    public string? Notes { get; private set; }

    public string? Source { get; private set; }

    public Difficulty Difficulty { get; private set; }

    public CookingProgram CookingProgram { get; private set; }

    public int PreparationTimeMinutes { get; private set; }

    public int CookingTimeMinutes { get; private set; }

    public int Servings { get; private set; }

    public IReadOnlyCollection<RecipeIngredient> Ingredients => _ingredients.AsReadOnly();

    public IReadOnlyCollection<RecipeStep> Steps => _steps.AsReadOnly();

    public IReadOnlyCollection<Category> Categories => _categories.AsReadOnly();

    public IReadOnlyCollection<Tag> Tags => _tags.AsReadOnly();

    public IReadOnlyCollection<Equipment> Equipments => _equipments.AsReadOnly();

    public IReadOnlyCollection<RecipeImage> Images => _images.AsReadOnly();

    private Recipe()
    {
    }

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

    #region Basic Information

    public void Rename(string name)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);

        name = name.Trim();

        if (name.Length > 200)
            throw new ArgumentException(
                "Recipe name cannot exceed 200 characters.",
                nameof(name));

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
                $"Ingredient '{ingredient.Name}' already exists.");
        }

        var recipeIngredient = new RecipeIngredient(
            this,
            ingredient,
            quantity,
            unit,
            notes);

        _ingredients.Add(recipeIngredient);

        ingredient.AddRecipeIngredient(recipeIngredient);

        return recipeIngredient;
    }

    public void RemoveIngredient(Guid ingredientId)
    {
        var ingredient = _ingredients
            .FirstOrDefault(i => i.IngredientId == ingredientId);

        if (ingredient is null)
            return;

        _ingredients.Remove(ingredient);

        ingredient.Ingredient.RemoveRecipeIngredient(ingredient);
    }

    #endregion

    #region Categories

    public void AddCategory(Category category)
    {
        ArgumentNullException.ThrowIfNull(category);

        if (_categories.Any(c => c.Id == category.Id))
            return;

        _categories.Add(category);

        category.AddRecipe(this);
    }

    public void RemoveCategory(Guid categoryId)
    {
        var category = _categories
            .FirstOrDefault(c => c.Id == categoryId);

        if (category is null)
            return;

        _categories.Remove(category);

        category.RemoveRecipe(this);
    }

    #endregion

    #region Tags

    public void AddTag(Tag tag)
    {
        ArgumentNullException.ThrowIfNull(tag);

        if (_tags.Any(t => t.Id == tag.Id))
            return;

        _tags.Add(tag);

        tag.AddRecipe(this);
    }

    public void RemoveTag(Guid tagId)
    {
        var tag = _tags
            .FirstOrDefault(t => t.Id == tagId);

        if (tag is null)
            return;

        _tags.Remove(tag);

        tag.RemoveRecipe(this);
    }

    #endregion

    #region Equipments

    public void AddEquipment(Equipment equipment)
    {
        ArgumentNullException.ThrowIfNull(equipment);

        if (_equipments.Any(e => e.Id == equipment.Id))
            return;

        _equipments.Add(equipment);

        equipment.AddRecipe(this);
    }

    public void RemoveEquipment(Guid equipmentId)
    {
        var equipment = _equipments
            .FirstOrDefault(e => e.Id == equipmentId);

        if (equipment is null)
            return;

        _equipments.Remove(equipment);

        equipment.RemoveRecipe(this);
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
        var step = _steps
            .FirstOrDefault(s => s.StepNumber == stepNumber);

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

        var step = _steps.First(s => s.StepNumber == from);

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
        if (isPrimary)
        {
            foreach (var image in _images)
            {
                image.RemovePrimary();
            }
        }

        var recipeImage = new RecipeImage(
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

        _images.Add(recipeImage);

        return recipeImage;
    }

    public void RemoveImage(Guid imageId)
    {
        var image = _images
            .FirstOrDefault(i => i.Id == imageId);

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

    private void ReorderImages()
    {
        for (var i = 0; i < _images.Count; i++)
        {
            _images[i].ChangeSortOrder(i);
        }
    }

    #endregion

    public override string ToString() => Name;
}