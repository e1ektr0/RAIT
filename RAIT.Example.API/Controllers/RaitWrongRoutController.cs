using Microsoft.AspNetCore.Mvc;
using RAIT.Example.API.Models;

namespace RAIT.Example.API.Controllers;

[ApiController]
[Route("[controller]/[action]")]
public class RaitWrongRoutController : ControllerBase
{
    [HttpGet]
    [Route("/rout")]
    public async Task<EnumOk> Get()
    {
        await Task.CompletedTask;
        return new EnumOk { EnumValue = TestEnum.Test1 };
    }
}

