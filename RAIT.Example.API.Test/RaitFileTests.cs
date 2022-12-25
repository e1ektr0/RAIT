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
        var model = new RaitFormFile("example.txt", "text");
        var responseModel =
            await _defaultClient.Call<RaitTestFileController, ResponseModel>(n => n.Post(model));

        Assert.That(responseModel!.Id, Is.EqualTo(10));
    }
}