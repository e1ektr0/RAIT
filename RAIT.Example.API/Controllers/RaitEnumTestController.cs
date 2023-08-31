using Microsoft.AspNetCore.Mvc;
using RAIT.Example.API.Models;

namespace RAIT.Example.API.Controllers;

[ApiController]
[Route("[controller]/[action]")]
public class RaitEnumTestController : ControllerBase
{
    [HttpGet]
    public async Task<EnumOk> Get()
    {
        await Task.CompletedTask;
        return new EnumOk { EnumValue = TestEnum.Test1 };
    }
}