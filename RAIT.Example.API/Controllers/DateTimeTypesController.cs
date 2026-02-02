using Microsoft.AspNetCore.Mvc;
using RAIT.Example.API.Models;

namespace RAIT.Example.API.Controllers;

[ApiController]
[Route("[controller]")]
public sealed class DateTimeTypesController : ControllerBase
{
    [HttpGet("date-only")]
    public Task<ActionResult<DateOnlyRange>> GetDateOnly([FromQuery] DateOnly fromDate, [FromQuery] DateOnly toDate)
        => Task.FromResult<ActionResult<DateOnlyRange>>(Ok(new DateOnlyRange { From = fromDate, To = toDate }));

    [HttpGet("date-time")]
    public Task<IActionResult> GetDateTime([FromQuery] DateTime focusDate)
        => Task.FromResult<IActionResult>(Ok());

    [HttpGet("date-time-offset")]
    public Task<IActionResult> GetDateTimeOffset([FromQuery] DateTimeOffset focusDate)
        => Task.FromResult<IActionResult>(Ok());
}
