using Ardalis.ApiEndpoints;
using Microsoft.AspNetCore.Mvc;
using RAIT.Example.API.Models;

namespace RAIT.Example.API.Endpoints;

public class ExampleEndpoint : EndpointBaseAsync
    .WithRequest<Model>
    .WithActionResult<Model>
{
    [HttpGet("GetTestEndpoint")]
    public override async Task<ActionResult<Model>> HandleAsync(Model request,
        CancellationToken cancellationToken = new())
    {
        await Task.CompletedTask;
        return new ActionResult<Model>(new Model
        {
            ExtraField = "test"
        });
    }
}