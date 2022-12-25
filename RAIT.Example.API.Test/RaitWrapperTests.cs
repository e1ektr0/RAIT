using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using RAIT.Core;
using RAIT.Example.API.Controllers;
using RAIT.Example.API.Models;

namespace RAIT.Example.API.Test;

public sealed class RaitWrapperTests
{
    private WebApplicationFactory<Program> _application = null!;
    private HttpClient _defaultClient = null!;
    private RaitHttpWrapper<RaitTestController> _raitHttpWrapper = null!;

    [SetUp]
    public void Setup()
    {
        _application = new WebApplicationFactory<Program>()
            .WithWebHostBuilder(PrepareEnv);

        _defaultClient = _application.CreateDefaultClient();
        _raitHttpWrapper = new RaitHttpWrapper<RaitTestController>(_defaultClient);
    }

    private void PrepareEnv(IWebHostBuilder _)
    {
        _.UseEnvironment("Test");
    }

    [Test]
    public async Task PostCall()
    {
        var model = new Model
        {
            Id = 10
        };
        var responseModel = await _raitHttpWrapper.Call(n => n.Post(model));

        Assert.That(responseModel!.Id, Is.EqualTo(10));
    }
}