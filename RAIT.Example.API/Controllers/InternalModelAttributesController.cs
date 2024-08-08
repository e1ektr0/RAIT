using Microsoft.AspNetCore.Mvc;
using RAIT.Example.API.Models;

namespace RAIT.Example.API.Controllers;

[ApiController]
[Route("[controller]/[action]")]
public class InternalModelAttributesController : ControllerBase
{
    [HttpPost("testInternalAttributes/{ExternalAccountId}")]
    public async Task<IActionResult> HttpPost(InternalModelAttributes model)
    {
        if (model.ExternalAccountId == null)
            throw new Exception();
        if (model.Model == null)
            throw new Exception();
        if (model.Model.Domain == null)
            throw new Exception();
        await Task.CompletedTask;
        return Ok();
    }
}