using Microsoft.AspNetCore.Mvc;
using RAIT.Example.API.Models;

namespace RAIT.Example.API.Controllers;

[ApiController]
[Route("[controller]")]
public class RaitTestController : ControllerBase
{
    [Route("route_parameter/{id}")]
    [HttpGet]
    public Task<Ok> GetWithId([FromRoute] long id)
    {
        if (id != 10)
            throw new Exception("Wrong value");
        return Task.FromResult(new Ok());
    }

    [Route("post_body")]
    [HttpPost]
    public Task<ResponseModel> Post([FromBody] Model model)
    {
        if (model.Id != 10)
            throw new Exception("Wrong value");
        return Task.FromResult(new ResponseModel { Id = 10 });
    }

    [Route("post_without_response")]
    [HttpPost]
    public Task PostWithoutResponse([FromBody] Model model)
    {
        if (model.Id != 10)
            throw new Exception("Wrong value");
        return Task.CompletedTask;
    }

    [Route("query_guid")]
    [HttpGet]
    public Task<Ok> GetFromQuery([FromQuery] Model model)
    {
        if (!model.Guid.HasValue)
            throw new Exception("Wrong value");
        return Task.FromResult(new Ok());
    }

    [Route("put_query")]
    [HttpPut]
    public Task<Ok> PutFromQuery([FromQuery] Model model)
    {
        if (model.Id != 10)
            throw new Exception("Wrong value");
        return Task.FromResult(new Ok());
    }

    [Route("sync_put")]
    [HttpPut]
    public Ok SyncPut([FromQuery] Model model)
    {
        if (model.Id != 10)
            throw new Exception("Wrong value");
        return new Ok();
    }

    [Route("delete_query")]
    [HttpDelete]
    public Task<Ok> DeleteQuery([FromQuery] long id)
    {
        if (id != 10)
            throw new Exception("Wrong value");
        return Task.FromResult(new Ok());
    }

    [Route("delete_query_named")]
    [HttpDelete]
    public Task<Ok> DeleteQueryNamed([FromQuery(Name = "name_id")] long id)
    {
        if (id != 10)
            throw new Exception("Wrong value");
        return Task.FromResult(new Ok());
    }

    [Route("form_model_null")]
    [HttpPost]
    public Task<ResponseModel> FormModelNull([FromForm] ModelWithNullValues model)
    {
        return Task.FromResult(new ResponseModel { Id = model.Id });
    }

    [Route("")]
    [HttpGet]
    public Task<Ok> Get()
    {
        return Task.FromResult(new Ok());
    }

    [Route("with_array")]
    [HttpGet]
    public Task<ArrayValueRequest> GetWithArray([FromQuery] ArrayValueRequest request)
    {
        return Task.FromResult(request);
    }

    [Route("with_date")]
    [HttpGet]
    public Task<DateTimeRequest> GetWithDate([FromQuery] DateTimeRequest request)
    {
        return Task.FromResult(request);
    }

    [Route("route_body/{id:long}")]
    [HttpPost]
    public Task<Ok> RouteBody([FromRoute] long id, [FromBody] Model request)
    {
        return Task.FromResult(new Ok());
    }

    [Route("route_query/{id:long}")]
    [HttpPost]
    public Task<Ok> RouteQuery([FromRoute] long id, [FromQuery] Model request)
    {
        if (id == 0 || request.Id == 0 || request.Guid == Guid.Empty || request.Domain == null)
            throw new Exception("Wrong value");
        return Task.FromResult(new Ok());
    }

    [Route("with_guid")]
    [HttpGet]
    public Task<Guid?> GetWithGuid([FromQuery] Guid? request)
    {
        return Task.FromResult(request);
    }
}
