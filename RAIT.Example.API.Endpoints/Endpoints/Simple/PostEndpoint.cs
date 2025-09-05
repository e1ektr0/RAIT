using Ardalis.ApiEndpoints;
using Microsoft.AspNetCore.Mvc;
using RAIT.Example.API.Endpoints.Endpoints.Simple.Models;

namespace RAIT.Example.API.Endpoints.Endpoints.Simple;

public class PostEndpoint
    : EndpointBaseAsync.WithRequest<PostRequest>.WithActionResult
{
    [HttpPost($"rout/{{{nameof(AggregatedGetRequest.ExternalAccountId)}}}/post")]
    public override async Task<ActionResult> HandleAsync(PostRequest request,
        CancellationToken cancellationToken = new())
    {
        if (request.ExternalAccountId == null)
            throw new Exception();
        if (request.Origin.ValueStr != "https://google.com")
            throw new Exception();
        if (request.Origin.Date.Year != 2000)
            throw new Exception();
        if (request.Test != "yyy")
            throw new Exception();
        await Task.CompletedTask;
        return Ok();
    }
}
