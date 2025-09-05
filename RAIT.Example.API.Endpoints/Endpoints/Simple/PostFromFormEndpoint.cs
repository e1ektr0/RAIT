using Ardalis.ApiEndpoints;
using Microsoft.AspNetCore.Mvc;
using RAIT.Example.API.Endpoints.Endpoints.Simple.Models;

namespace RAIT.Example.API.Endpoints.Endpoints.Simple;

public class PostFromFormEndpoint
    : EndpointBaseAsync.WithRequest<PostFromFormRequest>.WithActionResult
{
    [HttpPost($"rout/{{{nameof(AggregatedGetRequest.ExternalAccountId)}}}/postfromform")]
    public override async Task<ActionResult> HandleAsync(PostFromFormRequest request,
        CancellationToken cancellationToken = new())
    {
        if (request.Param1 != "param1" || request.Param2 != "param2")
            throw new Exception();
        await Task.CompletedTask;
        return Ok();
    }
}
