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
    public async Task<Ok> Post([FromBody] Model model)
    {
        await Task.CompletedTask;
        if (model.Id != 10)
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
    
    
    
    [Route("delete_query_test")]
    [HttpPut]
    public async Task<Ok> DeleteQuery([FromQuery] long id)
    {
        await Task.CompletedTask;
        if (id != 10)
            throw new Exception("Wrong value");
        return new Ok();
    }
}