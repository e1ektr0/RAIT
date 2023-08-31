using Microsoft.AspNetCore.Mvc;
using RAIT.Example.API.Models;

namespace RAIT.Example.API.Controllers;

[ApiController]
[Route("[controller]")]
public class RaitFilterTestController : ControllerBase
{
    [Route("{tenant}/reports/result")]
    [HttpGet]
    public async Task<Ok> GetReportsResult([FromRoute] string tenant, [FromQuery] string from,
        [FromQuery] string to)
    {
        if (tenant == null)
            throw new Exception("Parameter should not be null");
        
        // if (from == default)
        //     throw new Exception("Parameter should not be default");
        
        await Task.CompletedTask;
        return new Ok
        {
            Success = true
        };
    }
    
}