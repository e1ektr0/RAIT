using Microsoft.AspNetCore.Mvc;
using RAIT.Example.API.Models;

namespace RAIT.Example.API.Controllers;

[ApiController]
[Route("[controller]/[action]")]
public class RaitActionRoutingTestController : ControllerBase
{
    [HttpGet]
    public async Task<Ok> Get()
    {
        await Task.CompletedTask;
        return new Ok();
    }
}