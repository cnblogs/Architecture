using Asp.Versioning;
using Cnblogs.Architecture.Ddd.Cqrs.AspNetCore;
using Cnblogs.Architecture.Ddd.Infrastructure.Abstractions;

using Microsoft.AspNetCore.Mvc;

namespace Cnblogs.Architecture.IntegrationTestProject.Controllers;

[ApiVersion("1")]
[Route("/api/v{version:apiVersion}")]
public class TestController : ApiControllerBase
{
    [HttpGet("paging")]
    public Task<PagingParams?> PagingParamsAsync([FromQuery] PagingParams? pagingParams)
    {
        return Task.FromResult(pagingParams);
    }
}
