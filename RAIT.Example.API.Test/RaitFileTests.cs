using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using RAIT.Core;
using RAIT.Example.API.Controllers;
using RAIT.Example.API.Models;

namespace RAIT.Example.API.Test;

public sealed class RaitFileTests
{
    private WebApplicationFactory<Program> _application = null!;
    private HttpClient _defaultClient = null!;

    [SetUp]
    public void Setup()
    {
        _application = new WebApplicationFactory<Program>()
            .WithWebHostBuilder(PrepareEnv);

        _defaultClient = _application.CreateDefaultClient();
    }

    private void PrepareEnv(IWebHostBuilder _)
    {
        _.UseEnvironment("Test");
    }

    [Test]
    public async Task PostCall()
    {
        var model = new RaitFormFile("example.txt", "image/png");
        var responseModel =
            await _defaultClient.Rait<RaitTestFileController>().Call(n => n.Post(model));

        model.Dispose();
        Assert.That(responseModel!.Id, Is.EqualTo(10));
    }

    [Test]
    public async Task Post2Call()
    {
        var file = new RaitFormFile("example.txt", "image/png");
        var model = new Model { Id = 10, List = new List<Guid>{Guid.NewGuid()}};
        var responseModel = await _defaultClient.Rait<RaitTestFileController>().Call(n => n.Post3(model, file));
        file.Dispose();
        Assert.That(responseModel!.Id, Is.EqualTo(model.Id));
    }
    
    [Test]
    public async Task Post3Call()
    {
        var model = new RaitFormFile("example.txt", "image/png", await File.ReadAllBytesAsync("example2.txt"));
        var responseModel = await _defaultClient.Rait<RaitTestFileController>().Call(n => n.Post(model));
        model.Dispose();
        Assert.That(responseModel!.Id, Is.EqualTo(11));
    }
}