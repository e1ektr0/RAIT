using Microsoft.AspNetCore.Mvc;

namespace RAIT.Example.API.Controllers;

[ApiController]
[Route("[controller]")]
public class RaitPrimitiveTypesTestController : ControllerBase
{
    [Route("primitives/guid")]
    [HttpGet]
    public Task<Guid> GetGuid()
    {
        return Task.FromResult(Guid.Parse("6ec3e17e-c51c-43f0-b5d0-02889912a78c"));
    }

    [Route("primitives/int")]
    [HttpGet]
    public Task<int> GetInt()
    {
        return Task.FromResult(1);
    }

    [Route("primitives/string")]
    [HttpGet]
    public Task<string> GetString()
    {
        return Task.FromResult("test");
    }

    [Route("primitives/bool")]
    [HttpGet]
    public Task<bool> GetBool()
    {
        return Task.FromResult(true);
    }

    [Route("primitives/object")]
    [HttpGet]
    public Task<object> GetObject()
    {
        return Task.FromResult<object>("test");
    }
}
