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
        var created = new WeatherForecast { Id = 20 };
        await Task.CompletedTask;
        return TypedResults.Ok(created);
    }

    [HttpGet("GetCreated/{id:int}")]
    [ProducesResponseType(typeof(WeatherForecast), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<Created<WeatherForecast>> GetCreated(int id)
    {
        var created = new WeatherForecast { Id = 20 };
        await Task.CompletedTask;
        return TypedResults.Created($"/api/weather/{created.Id}", created);
    }

    // 1) Accepted (202)
    [HttpGet("GetAccepted")]
    public async Task<Accepted<WeatherForecast>> GetAccepted()
    {
        var wf = new WeatherForecast { Id = 42 };
        await Task.CompletedTask;
        // A location can be a relative or absolute URI
        return TypedResults.Accepted("/api/weather/42", wf);
    }

    // 2) NoContent (204)
    [HttpDelete("Delete/{id:int}")]
    public async Task<Results<NoContent, NotFound>> Delete(int id)
    {
        var exists = id > 0; // example check
        await Task.CompletedTask;
        return exists ? TypedResults.NoContent() : TypedResults.NotFound();
    }

    // 3) BadRequest (400)
    [HttpPost("CreateBadRequest")]
    public async Task<BadRequest<string>> CreateBadRequest()
    {
        await Task.CompletedTask;
        return TypedResults.BadRequest("Invalid payload or business rule violation");
    }

    // 4) Conflict (409)
    [HttpPost("CreateConflict")]
    public async Task<Conflict<string>> CreateConflict()
    {
        await Task.CompletedTask;
        return TypedResults.Conflict("A conflicting resource already exists.");
    }

    // 5) Unauthorized (401)
    [HttpGet("GetUnauthorized")]
    public async Task<UnauthorizedHttpResult> GetUnauthorized()
    {
        await Task.CompletedTask;
        return TypedResults.Unauthorized();
    }

    // 6) Unprocessable Entity (422)
    [HttpPost("Validate")]
    public async Task<UnprocessableEntity<string>> Validate()
    {
        await Task.CompletedTask;
        return TypedResults.UnprocessableEntity("Data failed validation.");
    }

    // 7) Generic StatusCode
    [HttpGet("Teapot")]
    public async Task<StatusCodeHttpResult> Teapot()
    {
        await Task.CompletedTask;
        return TypedResults.StatusCode(StatusCodes.Status418ImATeapot);
    }

    // 8) Problem details
    [HttpGet("GetProblem")]
    public async Task<ProblemHttpResult> GetProblem()
    {
        await Task.CompletedTask;
        return TypedResults.Problem("An unexpected error occurred.");
    }

    // 9) Json (custom serialization)
    [HttpGet("GetJson")]
    public async Task<JsonHttpResult<WeatherForecast>> GetJson()
    {
        var wf = new WeatherForecast { Id = 99 };
        await Task.CompletedTask;
        return TypedResults.Json(wf);
    }
}