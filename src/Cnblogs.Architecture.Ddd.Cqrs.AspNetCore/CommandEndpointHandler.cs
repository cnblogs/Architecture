using Cnblogs.Architecture.Ddd.Cqrs.Abstractions;
using MediatR;
using Microsoft.AspNetCore.Http;

namespace Cnblogs.Architecture.Ddd.Cqrs.AspNetCore;

/// <summary>
///     Execute command returned by endpoint handler, and then map command response to HTTP response.
/// </summary>
public class CommandEndpointHandler : IEndpointFilter
{
    private readonly IMediator _mediator;

    /// <summary>
    ///     Create a command endpoint handler.
    /// </summary>
    /// <param name="mediator"><see cref="IMediator"/></param>
    public CommandEndpointHandler(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <inheritdoc />
    public async ValueTask<object?> InvokeAsync(EndpointFilterInvocationContext context, EndpointFilterDelegate next)
    {
        var command = await next(context);
        if (command is null)
        {
            throw new InvalidOperationException(
                "Expected ICommand<>, but got null, check if your delegate in MapCommand(route, delegate) returned non-null command");
        }

        var response = await _mediator.Send(command);
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
                return Results.Ok(objectResponse.GetResult());
            }

            return Results.NoContent();
        }

        return HandleErrorCommandResponse(commandResponse);
    }

    private static IResult HandleErrorCommandResponse(CommandResponse response)
    {
        if (response.IsValidationError)
        {
            return Results.Text(response.ValidationError!.Message, statusCode: 400);
        }

        if (response is { IsConcurrentError: true, LockAcquired: false })
        {
            return Results.StatusCode(429);
        }

        return Results.Text(response.GetErrorMessage(), statusCode: 400);
    }
}
