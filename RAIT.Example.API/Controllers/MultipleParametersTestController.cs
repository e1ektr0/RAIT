using Microsoft.AspNetCore.Mvc;
using RAIT.Example.API.Models;

namespace RAIT.Example.API.Controllers;

[ApiController]
[Route("[controller]/[action]")]
public class MultipleParametersTestController : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> Get([FromRoute] Model model, [FromBody] Model model2)
    {
        await Task.CompletedTask;
        return Ok();
    }
}