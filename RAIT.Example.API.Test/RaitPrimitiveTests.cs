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
        var call = await _defaultClient.Call<RaitPrimitiveTypesTestController, int>(n => n.GetInt());
        Console.WriteLine(call);
    }

    [Test]
    public async Task GetString()
    {
        var call = await _defaultClient.Call<RaitPrimitiveTypesTestController, string>(n => n.GetString());
        Console.WriteLine(call);
    }

    [Test]
    public async Task GetGuid()
    {
        var call = await _defaultClient.Call<RaitPrimitiveTypesTestController, Guid>(n => n.GetGuid());
        Console.WriteLine(call);
    }
    
    [Test]
    public async Task GetBool()
    {
        var call = await _defaultClient.Call<RaitPrimitiveTypesTestController, bool>(n => n.GetBool());
        Console.WriteLine(call);
    }
}