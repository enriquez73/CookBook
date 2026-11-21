using Cookbook.Application.Common.Exceptions;
using FluentValidation;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.WebUtilities;

namespace Cookbook.Api.Middleware;

public sealed class GlobalExceptionHandler : IExceptionHandler
{
    private readonly ILogger<GlobalExceptionHandler> _logger;

    public GlobalExceptionHandler(
        ILogger<GlobalExceptionHandler> logger)
    {
        _logger = logger;
    }

    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken)
    {
        _logger.LogError(
            exception,
            "Unhandled exception.");

        var problem = exception switch
        {
            ValidationException validationException =>
                CreateValidationProblem(validationException),

            NotFoundException notFoundException =>
                CreateProblem(
                    StatusCodes.Status404NotFound,
                    notFoundException.Message),

            BadHttpRequestException badRequestException =>
                CreateProblem(
                    StatusCodes.Status400BadRequest,
                    badRequestException.Message),

            KeyNotFoundException =>
                CreateProblem(
                    StatusCodes.Status404NotFound,
                    "Resource not found."),

            UnauthorizedAccessException =>
                CreateProblem(
                    StatusCodes.Status401Unauthorized,
                    "Unauthorized."),

            _ =>
                CreateProblem(
                    StatusCodes.Status500InternalServerError,
                    "An unexpected error occurred.")
        };

        httpContext.Response.StatusCode = problem.Status!.Value;

        await httpContext.Response.WriteAsJsonAsync(
            problem,
            cancellationToken);

        return true;
    }

    private static ProblemDetails CreateProblem(
        int status,
        string detail)
    {
        return new ProblemDetails
        {
            Status = status,
            Title = ReasonPhrases.GetReasonPhrase(status),
            Detail = detail
        };
    }

    private static ValidationProblemDetails CreateValidationProblem(
        ValidationException exception)
    {
        return new ValidationProblemDetails(
            exception.Errors
                .GroupBy(e => e.PropertyName)
                .ToDictionary(
                    g => g.Key,
                    g => g
                        .Select(e => e.ErrorMessage)
                        .ToArray()))
        {
            Status = StatusCodes.Status400BadRequest,
            Title = "Validation failed."
        };
    }
}