using MediatR;

namespace Cookbook.Application.Recipes.Commands.DeleteRecipe;

public sealed record DeleteRecipeCommand(Guid Id) : IRequest;