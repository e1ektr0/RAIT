using System.Reflection;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Testing;
using RAIT.Core;
using RAIT.Example.API.Endpoints;
using RAIT.Example.API.Models;

namespace RAIT.Example.API.Test;

public sealed class RaitEndpointTests
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
    public async Task Get()
    {
        var model = new Model();
        var token = new CancellationToken();
        var actionResult = await _defaultClient.Rait<ExampleEndpoint>()
            .CallR(n => n.HandleAsync(model, token));

        Assert.That(actionResult.Value, Is.Not.Null);
        Assert.That(actionResult.Value.ExtraField, Is.EqualTo("test"));
    }

    [Test]
    public void Attributes()
    {
        var derivedType = typeof(ExampleEndpoint);
        var methodInfo = derivedType.GetMethod("HandleAsync")!;

        var attributes = methodInfo!.GetCustomAttributes(typeof(HttpGetAttribute), false);
        Assert.That(attributes, Is.Not.Empty);
    }
}