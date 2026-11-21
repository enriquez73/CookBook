using FluentValidation;

namespace Cookbook.Application.Recipes.Commands.CreateRecipe;

public sealed class CreateRecipeCommandValidator
    : AbstractValidator<CreateRecipeCommand>
{
    public CreateRecipeCommandValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty()
            .MaximumLength(200);

        RuleFor(x => x.Servings)
            .GreaterThan(0);

        RuleFor(x => x.PreparationTimeMinutes)
            .GreaterThanOrEqualTo(0);

        RuleFor(x => x.CookingTimeMinutes)
            .GreaterThan(0);
    }
}