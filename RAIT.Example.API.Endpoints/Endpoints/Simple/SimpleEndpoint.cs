using Ardalis.ApiEndpoints;
using Microsoft.AspNetCore.Mvc;

namespace RAIT.Example.API.Endpoints.Endpoints.Simple;

public class SimpleEndpoint : EndpointBaseAsync
    .WithoutRequest
    .WithoutResult
{
    [HttpGet("simple")]
    public override async Task<ActionResult> HandleAsync(CancellationToken cancellationToken = new())
    {
        await Task.CompletedTask;
        return Ok();
    }
}