using Ardalis.ApiEndpoints;
using Microsoft.AspNetCore.Mvc;
using RAIT.Example.API.Endpoints.Endpoints.Simple.Models;

namespace RAIT.Example.API.Endpoints.Endpoints.ActionResults;

public class GetEndpoint : EndpointBaseAsync
    .WithRequest<AggregatedGetRequest>
    .WithActionResult<ResponseDto>
{
    [HttpGet($"rout/{{{nameof(AggregatedGetRequest.ExternalAccountId)}}}/get")]
    public override async Task<ActionResult<ResponseDto>> HandleAsync(AggregatedGetRequest request,
        CancellationToken cancellationToken = new())
    {
        await Task.CompletedTask;
        var responseDto = new ResponseDto(request.ExternalAccountId, request.ValueStr);
        return new ActionResult<ResponseDto>(responseDto);
    }
}