using Microsoft.AspNetCore.Mvc;

namespace RAIT.Example.API.Endpoints.Endpoints.Simple.Models;
public class PostFromFormRequest
{
    [FromForm(Name = "param1")]
    public string? Param1 { get; init; }

    [FromForm(Name = "param2")]
    public string? Param2 { get; init; }
}
