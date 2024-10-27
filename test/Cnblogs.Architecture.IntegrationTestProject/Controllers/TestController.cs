using Asp.Versioning;
using Cnblogs.Architecture.Ddd.Cqrs.AspNetCore;
using Cnblogs.Architecture.Ddd.Infrastructure.Abstractions;
using Cnblogs.Architecture.IntegrationTestProject.Application.Commands;
using Cnblogs.Architecture.IntegrationTestProject.Application.Queries;
using Cnblogs.Architecture.IntegrationTestProject.Models;
using Cnblogs.Architecture.IntegrationTestProject.Payloads;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Cnblogs.Architecture.IntegrationTestProject.Controllers;

[ApiVersion("1")]
[Route("/api/v{version:apiVersion}/mvc")]
public class TestController(IMediator mediator) : ApiControllerBase
{
    [HttpGet("paging")]
    public Task<PagingParams?> PagingParamsAsync([FromQuery] PagingParams? pagingParams)
    {
        return Task.FromResult(pagingParams);
    }

    [HttpPut("strings/{id:int}")]
    public async Task<IActionResult> PutStringAsync(int id, [FromBody] UpdatePayload payload)
    {
        var response =
            await mediator.Send(new UpdateCommand(id, payload.NeedValidationError, payload.NeedExecutionError));
        return HandleCommandResponse(response);
    }

    [HttpGet("json/long-to-string/{id:long}")]
    public async Task<LongToStringModel?> GetLongToStringModelAsync(long id)
    {
        return await mediator.Send(new GetLongToStringQuery(id));
    }

    [HttpPost("json/long-to-string")]
    public async Task<IActionResult> CreateLongToStringModelAsync([FromBody] LongToStringModel model)
    {
        var response = await mediator.Send(new CreateLongToStringCommand(model.Id));
        return HandleCommandResponse(response);
    }
}
