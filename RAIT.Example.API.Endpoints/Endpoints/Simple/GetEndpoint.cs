using Ardalis.ApiEndpoints;
using Microsoft.AspNetCore.Mvc;
using RAIT.Example.API.Endpoints.Endpoints.Simple.Models;

namespace RAIT.Example.API.Endpoints.Endpoints.Simple;

public class GetEndpoint : EndpointBaseAsync
    .WithRequest<AggregatedGetRequest>
    .WithActionResult<ResponseDto>
{
    [HttpGet($"rout/{{{nameof(AggregatedGetRequest.ExternalAccountId)}}}/get")]
    public override async Task<ActionResult<ResponseDto>> HandleAsync(AggregatedGetRequest request,
        CancellationToken cancellationToken = new())
    {
        await Task.CompletedTask;
        if (request.Model?.Test != "test")
        {
            throw new Exception();
        }
        var responseDto = new ResponseDto(request.ExternalAccountId, request.ValueStr);
        return new ActionResult<ResponseDto>(responseDto);
    }
}
