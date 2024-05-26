using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using RAIT.Core;
using RAIT.Example.API.Controllers;
using RAIT.Example.API.Models;

namespace RAIT.Example.API.Test;

public sealed class RaitGetModelTests
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
    public async Task GetPing_Test()
    {
        var request = new Model
        {
            Id = 1,
            List = new List<Guid>
            {
                Guid.NewGuid(),
                Guid.NewGuid()
            },
            Domain = "google.com"
        };
        var response = await _defaultClient.Rait<RaitGetModelController>()
            .CallR(n => n.Ping(request));

        Assert.That(response.Id, Is.EqualTo(request.Id));
        Assert.That(response.List![1], Is.EqualTo(request.List[1]));
        Assert.That(response.Domain, Is.EqualTo(request.Domain));
    }


    [Test]
    public void Get415Error_Test()
    {
        var request = new Model
        {
            Id = 1,
            List = new List<Guid>
            {
                Guid.NewGuid(),
                Guid.NewGuid()
            }
        };
        Assert.ThrowsAsync<RaitHttpException>(async () =>
        {
            await _defaultClient.Rait<RaitGetModelController>()
                .CallR(n => n.WrongModelType(request));
        });
    }
}