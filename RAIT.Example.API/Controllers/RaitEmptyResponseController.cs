using Microsoft.AspNetCore.Mvc;

namespace RAIT.Example.API.Controllers;

[ApiController]
[Route("[controller]")]
public class RaitEmptyResponseController : ControllerBase
{
    [HttpPost]
    [Route("get_rout_parameter_test/{id}")]
    public async Task Post([FromRoute] long id)
    {
        await Task.CompletedTask;
        if (id != 10)
            throw new Exception("Wrong value");
    }
}