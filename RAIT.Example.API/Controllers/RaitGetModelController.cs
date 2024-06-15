using Microsoft.AspNetCore.Mvc;
using RAIT.Example.API.Models;

namespace RAIT.Example.API.Controllers;

[ApiController]
[Route("get_model")]
public class RaitGetModelController : ControllerBase
{
    /// <summary>
    /// Submit request to answer
    /// </summary>
    [HttpGet]
    [Route("Ping")]
    public async Task<Model> Ping([FromQuery] Model request)
    {
        await Task.CompletedTask;
        return request;
    }
    
    
    /// <summary>
    /// Submit request to answer
    /// </summary>
    [HttpPost]
    [Route("Ping")]
    public async Task<Model> PingPost([FromBody] Model request)
    {
        await Task.CompletedTask;
        return request;
    }
    
    /// <summary>
    /// Will return 415 error
    /// </summary>
    [HttpGet]
    [Route("WrongModelType")]
    public async Task<Model?> WrongModelType(Model request)
    {
        await Task.CompletedTask;
        return request;
    }
}