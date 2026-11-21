using FluentValidation;

namespace Cookbook.Application.Recipes.Commands.DeleteRecipe;

public sealed class DeleteRecipeCommandValidator
    : AbstractValidator<DeleteRecipeCommand>
{
    public DeleteRecipeCommandValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty();
    }
}