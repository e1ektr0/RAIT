using Microsoft.AspNetCore.Mvc;
using RAIT.Example.API.Models;

namespace RAIT.Example.API.Controllers;

[ApiController]
[Route("[controller]/[action]")]
public class RaitNullableTestController : ControllerBase
{
    [HttpGet]
    public async Task<Ok?> Get()
    {
        await Task.CompletedTask;
        return null;
    }

    [HttpPost]
    public async Task<Ok?> Post()
    {
        await Task.CompletedTask;
        return null;
    }
}