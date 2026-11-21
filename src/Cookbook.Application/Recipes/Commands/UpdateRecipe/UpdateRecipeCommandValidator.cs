using FluentValidation;

namespace Cookbook.Application.Recipes.Commands.UpdateRecipe;

public sealed class UpdateRecipeCommandValidator
    : AbstractValidator<UpdateRecipeCommand>
{
    public UpdateRecipeCommandValidator()
    {
        RuleFor(x => x.Id).NotEmpty();

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