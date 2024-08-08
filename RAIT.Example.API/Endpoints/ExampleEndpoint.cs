using Ardalis.ApiEndpoints;
using Microsoft.AspNetCore.Mvc;
using RAIT.Example.API.Models;

namespace RAIT.Example.API.Endpoints;

public class ExampleEndpoint : EndpointBaseAsync
    .WithRequest<InternalModelAttributes>
    .WithActionResult<AttributeResponseModel>
{
    
    [HttpPost("PostTestEndpoint/{ExternalAccountId}")]
    public override async Task<ActionResult<AttributeResponseModel>> HandleAsync(InternalModelAttributes request,
        CancellationToken cancellationToken = new())
    {
        await Task.CompletedTask;
        return new ActionResult<AttributeResponseModel>(new AttributeResponseModel
        {
            Domain = request.Model!.Domain,
            ExternalId = request.ExternalAccountId
        });
    }
}