using Microsoft.AspNetCore.Mvc;
using RAIT.Example.API.Models;

namespace RAIT.Example.API.Controllers;

[ApiController]
[Route("[controller]/[action]")]
public sealed class DateOnlyTestController : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<DateOnlyRange>> Get([FromQuery] DateOnly fromDate, [FromQuery] DateOnly toDate)
        => Ok(new DateOnlyRange { From = fromDate, To = toDate });
}