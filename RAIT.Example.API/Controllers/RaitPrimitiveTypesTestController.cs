using Microsoft.AspNetCore.Mvc;

namespace RAIT.Example.API.Controllers;

[ApiController]
[Route("[controller]")]
public class RaitPrimitiveTypesTestController : ControllerBase
{
    [Route("get_rout_parameter_test/get_guid")]
    [HttpGet]
    public async Task<Guid> GetGuid()
    {
        await Task.CompletedTask;
        return Guid.Parse("6ec3e17e-c51c-43f0-b5d0-02889912a78c");
    }
    
    [Route("get_rout_parameter_test/get_int")]
    [HttpGet]
    public async Task<int> GetInt()
    {
        await Task.CompletedTask;
        return 1;
    }
    
    [Route("get_rout_parameter_test/get_string")]
    [HttpGet]
    public async Task<string> GetString()
    {
        await Task.CompletedTask;
        return "test";
    }
    
    [Route("get_rout_parameter_test/get_bool")]
    [HttpGet]
    public async Task<bool> GetBool()
    {
        await Task.CompletedTask;
        return true;
    }
    
    [Route("get_rout_parameter_test/get_object")]
    [HttpGet]
    public async Task<object> GetObject()
    {
        await Task.CompletedTask;
        return "test";
    }
}