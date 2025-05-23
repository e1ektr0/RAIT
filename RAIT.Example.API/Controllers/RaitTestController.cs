using Microsoft.AspNetCore.Mvc;
using RAIT.Example.API.Models;

namespace RAIT.Example.API.Controllers;

[ApiController]
[Route("[controller]")]
public class RaitTestController : ControllerBase
{
    [Route("get_rout_parameter_test/{id}")]
    [HttpGet]
    public async Task<Ok> GetWithId([FromRoute] long id)
    {
        await Task.CompletedTask;
        if (id != 10)
            throw new Exception("Wrong value");
        return new Ok();
    }


    [Route("post_body_test")]
    [HttpPost]
    public async Task<ResponseModel> Post([FromBody] Model model)
    {
        await Task.CompletedTask;
        if (model.Id != 10)
            throw new Exception("Wrong value");
        return new ResponseModel
        {
            Id = 10
        };
    }
    
    
    [Route("post_withoutresp_body_test")]
    [HttpPost]
    public async Task PostWithoutResponse([FromBody] Model model)
    {
        await Task.CompletedTask;
        if (model.Id != 10)
            throw new Exception("Wrong value");
    }

    [Route("get_query_guid_test")]
    [HttpGet]
    public async Task<Ok> GetFromQuery([FromQuery] Model model)
    {
        await Task.CompletedTask;
        if (!model.Guid.HasValue)
            throw new Exception("Wrong value");
        return new Ok();
    }

    [Route("put_query_test")]
    [HttpPut]
    public async Task<Ok> PutFromQuery([FromQuery] Model model)
    {
        await Task.CompletedTask;
        if (model.Id != 10)
            throw new Exception("Wrong value");
        return new Ok();
    }

    [Route("sync_put")]
    [HttpPut]
    public Ok SyncPut([FromQuery] Model model)
    {
        if (model.Id != 10)
            throw new Exception("Wrong value");
        return new Ok();
    }

    [Route("delete_query_test")]
    [HttpDelete]
    public async Task<Ok> DeleteQuery([FromQuery] long id)
    {
        await Task.CompletedTask;
        if (id != 10)
            throw new Exception("Wrong value");
        return new Ok();
    }

    [Route("delete_query_test_named")]
    [HttpDelete]
    public async Task<Ok> DeleteQueryNamed([FromQuery(Name = "name_id")] long id)
    {
        await Task.CompletedTask;
        if (id != 10)
            throw new Exception("Wrong value");
        return new Ok();
    }

    [Route("form_model_with-null_value")]
    [HttpPost]
    public async Task<ResponseModel> FormModelNull([FromForm] ModelWithNullValues model)
    {
        await Task.CompletedTask;
        return new ResponseModel
        {
            Id = model.Id
        };
    }

    [Route("")]
    [HttpGet]
    public async Task<Ok> Get()
    {
        await Task.CompletedTask;
        return new Ok();
    }

    [Route("get_with_array")]
    [HttpGet]
    public async Task<ArrayValueRequest> GetWithArray([FromQuery] ArrayValueRequest request)
    {
        await Task.CompletedTask;
        return request;
    }

    [Route("get_with_date")]
    [HttpGet]
    public async Task<DateTimeRequest> GetWithDate([FromQuery] DateTimeRequest request)
    {
        await Task.CompletedTask;
        return request;
    }

    [Route("route-body/{id:long}")]
    [HttpPost]
    public async Task<Ok> RouteBody([FromRoute] long id, [FromBody] Model request)
    {
        await Task.CompletedTask;
        return new Ok();
    }

    [Route("route-query/{id:long}")]
    [HttpPost]
    public async Task<Ok> RouteQuery([FromRoute] long id, [FromQuery] Model request)
    {
        await Task.CompletedTask;
        if (id == default || request.Id == default || request.Guid == Guid.Empty || request.Domain == default)
            throw new Exception("Wrong value");
        return new Ok();
    }
    
    [Route("get_with_giud")]
    [HttpGet]
    public async Task<Guid?> GetWithGuid([FromQuery] Guid? request)
    {
        await Task.CompletedTask;
        return request;
    }
}