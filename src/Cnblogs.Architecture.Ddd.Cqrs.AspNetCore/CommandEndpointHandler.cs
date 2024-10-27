using Cnblogs.Architecture.Ddd.Cqrs.Abstractions;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;

namespace Cnblogs.Architecture.Ddd.Cqrs.AspNetCore;

/// <summary>
///     Execute command returned by endpoint handler, and then map command response to HTTP response.
/// </summary>
public class CommandEndpointHandler(IMediator mediator, IOptions<CqrsHttpOptions> options) : IEndpointFilter
{
    private readonly CqrsHttpOptions _options = options.Value;

    /// <inheritdoc />
    public async ValueTask<object?> InvokeAsync(EndpointFilterInvocationContext context, EndpointFilterDelegate next)
    {
        var command = await next(context);
        if (command is null)
        {
            throw new InvalidOperationException(
                "Expected ICommand<>, but got null, check if your delegate in MapCommand(route, delegate) returned non-null command");
        }

        if (command is not IBaseRequest)
        {
            // not command, return as-is
            return command;
        }

        var response = await mediator.Send(command);
        if (response is null)
        {
            // should not be null
            throw new InvalidOperationException($"Response to {command.GetType().Name} is null");
        }

        if (response is not CommandResponse commandResponse)
        {
            throw new InvalidOperationException(
                $"response for request({response.GetType().Name}) is not CommandResponse");
        }

        if (commandResponse.IsSuccess())
        {
            // check if response has result
            if (commandResponse is IObjectResponse objectResponse)
            {
                return context.HttpContext.Request.Headers.CqrsVersion() > 1
                    ? Results.Extensions.Cqrs(response, _options.DefaultJsonSerializerOptions)
                    : Results.Json(objectResponse.GetResult(), _options.DefaultJsonSerializerOptions);
            }

            return Results.NoContent();
        }

        return HandleErrorCommandResponse(commandResponse, context.HttpContext);
    }

    private IResult HandleErrorCommandResponse(CommandResponse response, HttpContext context)
    {
        var errorResponseType = _options.CommandErrorResponseType;
        if (context.Request.Headers.Accept.Contains("application/cqrs")
            || context.Request.Headers.Accept.Contains("application/cqrs-v2"))
        {
            errorResponseType = ErrorResponseType.Cqrs;
        }

        return errorResponseType switch
        {
            ErrorResponseType.PlainText => HandleErrorCommandResponseWithPlainText(response),
            ErrorResponseType.ProblemDetails => HandleErrorCommandResponseWithProblemDetails(response),
            ErrorResponseType.Cqrs => HandleErrorCommandResponseWithCqrs(response),
            ErrorResponseType.Custom => _options.CustomCommandErrorResponseMapper?.Invoke(response, context)
                                        ?? HandleErrorCommandResponseWithPlainText(response),
            _ => throw new ArgumentOutOfRangeException(
                $"Unsupported CommandErrorResponseType: {_options.CommandErrorResponseType}")
        };
    }

    private static IResult HandleErrorCommandResponseWithCqrs(CommandResponse response)
    {
        if (response is { IsConcurrentError: true, LockAcquired: false })
        {
            return Results.StatusCode(429);
        }

        return Results.BadRequest((object)response);
    }

    private static IResult HandleErrorCommandResponseWithPlainText(CommandResponse response)
    {
        if (response.IsValidationError)
        {
            return Results.Text(string.Join('\n', response.ValidationErrors.Select(x => x.Message)), statusCode: 400);
        }

        if (response is { IsConcurrentError: true, LockAcquired: false })
        {
            return Results.StatusCode(429);
        }

        return Results.Text(response.GetErrorMessage(), statusCode: 400);
    }

    private static IResult HandleErrorCommandResponseWithProblemDetails(CommandResponse response)
    {
        if (response.IsValidationError)
        {
            var errors = response.ValidationErrors
                .GroupBy(x => x.ParameterName ?? "command")
                .ToDictionary(x => x.Key, x => x.Select(y => y.Message).ToArray());
            return Results.ValidationProblem(errors, statusCode: 400);
        }

        if (response is { IsConcurrentError: true, LockAcquired: false })
        {
            return Results.Problem(
                "The lock can not be acquired within time limit, please try later.",
                statusCode: 429,
                title: "Concurrent error");
        }

        return Results.Problem(response.GetErrorMessage(), statusCode: 400, title: "Execution failed");
    }
}
