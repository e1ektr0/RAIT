using System.Net;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using RAIT.Core;
using RAIT.Example.API.Controllers;

namespace RAIT.Example.API.Test;

public sealed class RaitTypedTests
{
    private WebApplicationFactory<Program> _application = null!;
    private HttpClient _defaultClient = null!;

    [SetUp]
    public void Setup()
    {
        _application = new WebApplicationFactory<Program>()
            .WithWebHostBuilder(PrepareEnv);

        _defaultClient = _application.CreateDefaultClient();
        _application.Services.ConfigureRait();
    }

    private void PrepareEnv(IWebHostBuilder builder)
    {
        builder.ConfigureTestServices(s => s.AddRait());
        builder.UseEnvironment("Test");
    }


    [Test]
    public async Task GetAsyncResults_ReturnsOk()
    {
        var response = await _defaultClient
            .Rait<TypedController>()
            .CallR(c => c.GetAsyncResults(1));

    }

    [Test]
    public async Task GetCreated_ReturnsCreated()
    {
        var response = await _defaultClient
            .Rait<TypedController>()
            .CallR(c => c.GetCreated(10));

        Assert.That(response.Value!.Id, Is.EqualTo(20));
    }
}