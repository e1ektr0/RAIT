using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using RAIT.Core;
using RAIT.Example.API.Controllers;
using RAIT.Example.API.Models;

namespace RAIT.Example.API.Test;

public sealed class RaitTests
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
    public async Task GetCall()
    {
        await _defaultClient.Call<RaitTestController, Ok>(n => n.GetWithId(10));
    }

    [Test]
    public async Task PostCall()
    {
        var model = new Model
        {
            Id = 10
        };
        await _defaultClient.Call<RaitTestController, Ok>(n => n.Post(model));
    }
    
    
    [Test]
    public async Task PutFromQueryCall()
    {
        var model = new Model
        {
            Id = 10
        };
        await _defaultClient.Call<RaitTestController, Ok>(n => n.PutFromQuery(model));
    }
    [Test]
    public async Task DeleteFromQueryCall()
    {
        await _defaultClient.Call<RaitTestController, Ok>(n => n.DeleteQuery(10));
    }
}