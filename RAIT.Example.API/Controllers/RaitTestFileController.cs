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
        if (file.FileName == null)
            throw new Exception("Wrong value");
        return new ResponseModel
        {
            Id = 10
        };
    }
    [Route("post_file_test_with_model")]
    [HttpPost]
    public async Task<ResponseModel> Post([FromForm]ModelWithFile model)
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

