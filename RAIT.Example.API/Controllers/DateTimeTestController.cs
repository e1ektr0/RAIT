using Microsoft.AspNetCore.Mvc;

namespace RAIT.Example.API.Controllers;

[ApiController]
[Route("[controller]/[action]")]
public class DateTimeTestController : ControllerBase
{

    [HttpGet]
    public async Task<IActionResult> Create(
        [FromQuery] DateTime focusDate)
    {
        await Task.CompletedTask;
        return Ok();
    }
}