using Ardalis.ApiEndpoints;
using Microsoft.AspNetCore.Mvc;
using RAIT.Example.API.Endpoints.Endpoints.FromQuery.Models;
using RAIT.Example.API.Endpoints.Endpoints.Simple.Models;

namespace RAIT.Example.API.Endpoints.Endpoints.FromQuery;

public class FromQueryEndpoint : EndpointBaseAsync
    .WithRequest<OperationRequest<CompanyModel>>
    .WithActionResult<ResponseDto>
{
    [HttpGet($"frm_get")]
    public override async Task<ActionResult<ResponseDto>> HandleAsync([FromQuery]OperationRequest<CompanyModel> request,
        CancellationToken cancellationToken = new())
    {
        await Task.CompletedTask;
        if (request.Origin?.Test != "test")
        {
            throw new Exception();
        }

        var responseDto = new ResponseDto("ext", "val");
        return new ActionResult<ResponseDto>(responseDto);
    }
}