using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using RAIT.Example.API.Models;

namespace RAIT.Example.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class TypedController : ControllerBase
{

    
    [HttpGet("GetAsyncResults/{id:int}")]
    [ProducesResponseType(typeof(WeatherForecast), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<Results<Ok<WeatherForecast>, NotFound<string>>> GetAsyncResults(int id)
    {
        var created = new WeatherForecast
        {
            Id = 20
        };
        await Task.CompletedTask;

        return TypedResults.Ok(created);
    }
    
    [HttpGet("GetCreated/{id:int}")]
    [ProducesResponseType(typeof(WeatherForecast), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<Created<WeatherForecast>> GetCreated(int id)
    {
        var created = new WeatherForecast
        {
            Id = 20
        };

        await Task.CompletedTask;
        return TypedResults.Created($"/api/weather/{created.Id}", created);
    }
   
}