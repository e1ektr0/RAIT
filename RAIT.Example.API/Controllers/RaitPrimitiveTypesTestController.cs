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
        return Guid.NewGuid();
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
}