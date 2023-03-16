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
        var responseModel = await _defaultClient.Call<RaitTestController, ResponseModel>(n => n.Post(model));

        Assert.That(responseModel!.Id, Is.EqualTo(10));
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

    [Test]
    public async Task ActionRoutingTest()
    {
        await _defaultClient.Call<RaitActionRoutingTestController, Ok>(n => n.Get(1));
    }

    [Test]
    public async Task FilterTest()
    {
        var call = await _defaultClient.Call<RaitFilterTestController, Ok>(n =>
            n.GetReportsResult("fff", DateTime.Now, DateTime.Now));
        Assert.That(call!.Success, Is.True);
    }

    [Test]
    public async Task NullableTest()
    {
        var rait = new RaitHttpWrapper<RaitNullableTestController>(_defaultClient);
        await rait.Call(n => n.Get());
        await rait.Call(n => n.Post());
    }  
    
    [Test]
    public async Task ActionResultTest()
    {
        var rait = new RaitHttpWrapper<RaitActionResultTestController>(_defaultClient);
        await rait.Call(n => n.Get());
    }
}