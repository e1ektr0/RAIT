using System.Text;
using Microsoft.AspNetCore.Mvc;
using RAIT.Example.API.Models;

namespace RAIT.Example.API.Controllers;

[ApiController]
[Route("[controller]")]
public class RaitTestFileController : ControllerBase
{
    [Route("post_file_test")]
    [HttpPost]
    public async Task<ResponseModel> Post(IFormFile file)
    {
        await Task.CompletedTask;
        var readAsStringAsync = await ReadAsStringAsync(file);
        if (!readAsStringAsync.StartsWith("10"))
            throw new Exception("Wrong value");
        return new ResponseModel
        {
            Id = 10
        };
    }

    private static async Task<string> ReadAsStringAsync(IFormFile file)
    {
        var result = new StringBuilder();
        using (var reader = new StreamReader(file.OpenReadStream()))
        {
            while (reader.Peek() >= 0)
                result.AppendLine(await reader.ReadLineAsync());
        }

        return result.ToString();
    }

    [Route("post_file_test_with_model")]
    [HttpPost]
    public async Task<ResponseModel> Post([FromForm] ModelWithFile model)
    {
        await Task.CompletedTask;
        if (model.File.FileName == null)
            throw new Exception("Wrong value");
        return new ResponseModel
        {
            Id = 10
        };
    }
}