using Microsoft.AspNetCore.Mvc;

namespace RAIT.Example.API.Controllers;

[ApiController]
[Route("[controller]")]
public class RaitEmptyResponseController : ControllerBase
{
    [HttpPost]
    [Route("route_parameter_test/{id}")]
    public Task Post([FromRoute] long id)
    {
        if (id != 10)
            throw new Exception("Wrong value");
        return Task.CompletedTask;
    }
}
