using Ardalis.ApiEndpoints;
using Microsoft.AspNetCore.Mvc;
using RAIT.Example.API.Models;

namespace RAIT.Example.API.Endpoints;

public class ExampleEndpoint : EndpointBaseAsync
    .WithRequest<Model>
    .WithActionResult<AttributeResponseModel>
{
    
    [HttpPost("PostTestEndpoint")]
    public override async Task<ActionResult<AttributeResponseModel>> HandleAsync(Model request,
        CancellationToken cancellationToken = new())
    {
        await Task.CompletedTask;
        return new ActionResult<AttributeResponseModel>(new AttributeResponseModel
        {
            Id = "test"
        });
    }
}