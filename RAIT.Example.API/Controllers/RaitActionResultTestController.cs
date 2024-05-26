using Microsoft.AspNetCore.Mvc;

namespace RAIT.Example.API.Controllers;

[ApiController]
[Route("[controller]/[action]")]
public class RaitActionResultTestController : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> Get()
    {
        await Task.CompletedTask;
        return Ok();
    }
    
    [HttpGet]
    public async Task<IActionResult> Get401StatusCode()
    {
        await Task.CompletedTask;
        return StatusCode(401);
    }

    [HttpPost]
    public async Task<IActionResult> Post(int a)
    {
        await Task.CompletedTask;
        return Ok();
    }
}