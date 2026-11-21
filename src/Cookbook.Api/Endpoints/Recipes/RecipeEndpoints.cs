using Cookbook.Api.Configuration;
using Cookbook.Application.Common.Models;
using Cookbook.Application.Recipes.Commands.CreateRecipe;
using Cookbook.Application.Recipes.Commands.DeleteRecipe;
using Cookbook.Application.Recipes.Commands.UpdateRecipe;
using Cookbook.Application.Recipes.DTOs;
using Cookbook.Application.Recipes.Queries.GetRecipe;
using Cookbook.Application.Recipes.Queries.GetRecipes;
using MediatR;
using Microsoft.AspNetCore.Http.HttpResults;

namespace Cookbook.Api.Endpoints.Recipes;

public sealed class RecipeEndpoints : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        var group = app.MapGroup(ApiRoutes.Recipes.Base)
            .WithTags(ApiTags.Recipes);

        group.MapGet("/", GetRecipes)
            .WithName("GetRecipes")
            .WithSummary("Gets recipes")
            .Produces<PagedResult<RecipeListItemDto>>();

        group.MapGet("/{id:guid}", GetRecipe)
            .WithName("GetRecipe")
            .WithSummary("Gets recipe by id")
            .Produces<RecipeDto>()
            .Produces(StatusCodes.Status404NotFound);

        group.MapPost("/", CreateRecipe)
            .WithName("CreateRecipe")
            .WithSummary("Creates a recipe")
            .Produces<CreateRecipeResponse>(StatusCodes.Status201Created)
            .ProducesValidationProblem();

        group.MapPut("/{id:guid}", UpdateRecipe);

        group.MapDelete("/{id:guid}", DeleteRecipe)
            .WithName("DeleteRecipe")
            .WithSummary("Deletes a recipe")
            .Produces(StatusCodes.Status204NoContent)
            .Produces(StatusCodes.Status404NotFound);
    }

    private static async Task<Ok<PagedResult<RecipeListItemDto>>> GetRecipes(
        [AsParameters] GetRecipesRequest request,
        ISender sender,
        CancellationToken cancellationToken)
    {
        var result = await sender.Send(
            new GetRecipesQuery(request.Page, request.PageSize),
            cancellationToken);

        return TypedResults.Ok(result);
    }

    private static async Task<Ok<RecipeDto>> GetRecipe(
        Guid id,
        ISender sender,
        CancellationToken cancellationToken)
    {
        var result = await sender.Send(
            new GetRecipeQuery(id),
            cancellationToken);

        return TypedResults.Ok(result);
    }

    private static async Task<Created<CreateRecipeResponse>> CreateRecipe(
        CreateRecipeCommand command,
        ISender sender,
        CancellationToken cancellationToken)
    {
        var result = await sender.Send(command, cancellationToken);

        return TypedResults.Created(
            $"{ApiRoutes.Recipes.Base}/{result.Id}",
            result);
    }

    private static async Task<NoContent> UpdateRecipe(
        Guid id,
        UpdateRecipeCommand command,
        ISender sender,
        CancellationToken cancellationToken)
    {
        if (id != command.Id)
            throw new BadHttpRequestException("Route id and body id do not match.");

        await sender.Send(command, cancellationToken);

        return TypedResults.NoContent();
    }

    private static async Task<NoContent> DeleteRecipe(
        Guid id,
        ISender sender,
        CancellationToken cancellationToken)
    {
        await sender.Send(
            new DeleteRecipeCommand(id),
            cancellationToken);
        
        return TypedResults.NoContent();
    }

    public sealed record GetRecipesRequest(
        int Page = 1,
        int PageSize = 20);
}