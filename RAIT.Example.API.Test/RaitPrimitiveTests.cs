using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using RAIT.Core;
using RAIT.Example.API.Controllers;

namespace RAIT.Example.API.Test;

public sealed class RaitPrimitiveTests
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
    public async Task GetInt()
    {
        var call = await _defaultClient.Rait<RaitPrimitiveTypesTestController>().Call(n => n.GetInt());
        Console.WriteLine(call);
    }

    [Test]
    public async Task GetString()
    {
        var call = await _defaultClient.Rait<RaitPrimitiveTypesTestController>().Call(n => n.GetString());
        Console.WriteLine(call);
    }

    [Test]
    public async Task GetGuid()
    {
        var call = await _defaultClient.Rait<RaitPrimitiveTypesTestController>().Call(n => n.GetGuid());
        Console.WriteLine(call);
    }

    [Test]
    public async Task GetBool()
    {
        var call = await _defaultClient.Rait<RaitPrimitiveTypesTestController>().Call(n => n.GetBool());
        Console.WriteLine(call);
    }

    [Test]
    public async Task GetObject()
    {
        var call = await _defaultClient.Rait<RaitPrimitiveTypesTestController>().Call(n => n.GetObject());
        Console.WriteLine(call);
    }
}