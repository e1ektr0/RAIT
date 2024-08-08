using Microsoft.AspNetCore.Mvc;
using RAIT.Example.API.Models;

namespace RAIT.Example.API.Controllers;

[ApiController]
[Route("[controller]")]
public class RaitAttributesController : ControllerBase
{
    [Route("post_body_test")]
    [HttpPost]
    public async Task<AttributeResponseModel> Ping([FromBody] AttributeModel model)
    {
        await Task.CompletedTask;
      
        return new AttributeResponseModel
        {
            Domain = model.Id
        };
    }
}