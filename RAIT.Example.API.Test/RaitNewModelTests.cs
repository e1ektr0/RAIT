using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using RAIT.Core;
using RAIT.Example.API.Controllers;
using RAIT.Example.API.Models;

namespace RAIT.Example.API.Test;

public sealed class RaitNewModelTests
{
    private WebApplicationFactory<Program> _application = null!;
    private HttpClient _httpClient = null!;

    [SetUp]
    public void Setup()
    {
        _application = new WebApplicationFactory<Program>()
            .WithWebHostBuilder(PrepareEnv);

        _httpClient = _application.CreateDefaultClient();
    }

    private void PrepareEnv(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Test");
    }

    /// <summary>
    /// 
    /// </summary>
    /// <example>example value</example>
    [Test]
    public async Task Post_ValidModel_ReturnsExpectedResult()
    {
        var model = new Model
        {
            Id = 10,
            Domain = "test",
            ExtraField = "extra"
        };
        var responseModel = await _httpClient.Rait<RaitTestController>().Call(n => n.Post(model));

        Assert.That(responseModel!.Id, Is.EqualTo(10));
    }
}