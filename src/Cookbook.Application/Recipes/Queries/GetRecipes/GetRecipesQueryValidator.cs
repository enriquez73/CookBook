using FluentValidation;

namespace Cookbook.Application.Recipes.Queries.GetRecipes;

public sealed class GetRecipesQueryValidator
    : AbstractValidator<GetRecipesQuery>
{
    public GetRecipesQueryValidator()
    {
        RuleFor(x => x.Page)
            .GreaterThan(0);

        RuleFor(x => x.PageSize)
            .InclusiveBetween(1, 100);
    }
}