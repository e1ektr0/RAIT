using Microsoft.AspNetCore.Mvc;
using RAIT.Example.API.Models;

namespace RAIT.Example.API.Controllers;

[ApiController]
[Route("[controller]/[action]")]
public class RaitWrongRouteController : ControllerBase
{
    [HttpGet]
    [Route("/route")]
    public Task<EnumOk> Get()
    {
        return Task.FromResult(new EnumOk { EnumValue = TestEnum.Test1 });
    }
}
