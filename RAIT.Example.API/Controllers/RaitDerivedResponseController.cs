using Microsoft.AspNetCore.Mvc;
using RAIT.Example.API.Models;

namespace RAIT.Example.API.Controllers;

[ApiController]
[Route("[controller]")]
public class RaitDerivedResponseController : ControllerBase
{
    [Route("test")]
    [HttpGet]
    public Task<BaseResp> GetChildResp()
    {
        return Task.FromResult<BaseResp>(new ChildResp { BaseProp = "123", ChildProp = "321" });
    }
}