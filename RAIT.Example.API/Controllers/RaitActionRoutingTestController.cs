using Microsoft.AspNetCore.Mvc;
using RAIT.Example.API.Models;

namespace RAIT.Example.API.Controllers;

[ApiController]
[Route("[controller]/[action]")]
public class RaitActionRoutingTestController : ControllerBase
{
    [HttpGet]
    public async Task<Ok> Get(int id)
    {
        if (id == 0)
            throw new Exception("Should not be default");
        await Task.CompletedTask;
        return new Ok();
    }
    
}