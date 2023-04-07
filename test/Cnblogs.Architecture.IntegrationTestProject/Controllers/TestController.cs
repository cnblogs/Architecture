using Asp.Versioning;
using Cnblogs.Architecture.Ddd.Cqrs.AspNetCore;
using Cnblogs.Architecture.Ddd.Infrastructure.Abstractions;
using Cnblogs.Architecture.IntegrationTestProject.Application.Commands;
using Cnblogs.Architecture.IntegrationTestProject.Payloads;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Cnblogs.Architecture.IntegrationTestProject.Controllers;

[ApiVersion("1")]
[Route("/api/v{version:apiVersion}/mvc")]
public class TestController : ApiControllerBase
{
    private readonly IMediator _mediator;

    public TestController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet("paging")]
    public Task<PagingParams?> PagingParamsAsync([FromQuery] PagingParams? pagingParams)
    {
        return Task.FromResult(pagingParams);
    }

    [HttpPut("strings/{id:int}")]
    public async Task<IActionResult> PutStringAsync(int id, [FromBody] UpdatePayload payload)
    {
        var response =
            await _mediator.Send(new UpdateCommand(id, payload.NeedValidationError, payload.NeedExecutionError));
        return HandleCommandResponse(response);
    }
}
