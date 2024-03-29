using System.Text.Json;
using System.Text.Json.Serialization;
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
        await _defaultClient.Rait<RaitTestController>().Call(n => n.GetWithId(10));
    }

    [Test]
    public async Task PostCall()
    {
        var model = new Model
        {
            Id = 10
        };
        var responseModel = await _defaultClient.Rait<RaitTestController>().Call(n => n.Post(model));

        Assert.That(responseModel!.Id, Is.EqualTo(10));
    }

    [Test]
    public async Task PutFromQueryCall()
    {
        var model = new Model
        {
            Id = 10
        };
        await _defaultClient.Rait<RaitTestController>().Call(n => n.PutFromQuery(model));
    }

    [Test]
    public async Task DeleteFromQueryCall()
    {
        await _defaultClient.Rait<RaitTestController>().Call(n => n.DeleteQuery(10));
    }

    [Test]
    public async Task ActionRoutingTest()
    {
        await _defaultClient.Rait<RaitActionRoutingTestController>().Call(n => n.Get(1));
    }

    [Test]
    public async Task FilterTest()
    {
        var call = await _defaultClient.Rait<RaitFilterTestController>().Call(n =>
            n.GetReportsResult("fff", "qqq","sss"));
        Assert.That(call!.Success, Is.True);
    }

    [Test]
    public async Task NullableTest()
    {
        await _defaultClient.Rait<RaitNullableTestController>().Call(n => n.Get());
        await _defaultClient.Rait<RaitNullableTestController>().Call(n => n.Post());
    }

    [Test]
    public async Task ActionResultTest()
    {
        var r1 = await _defaultClient.Rait<RaitActionResultTestController>().Call(n => n.Get());
        var r2 = await _defaultClient.Rait<RaitActionResultTestController>().Call(n => n.Get2());
        await _defaultClient.Rait<RaitActionResultTestController>().Call(n => n.Post(1));
    }

    [Test]
    public async Task ResponseEnumTest()
    {
        RaitConfig.SerializationOptions = new JsonSerializerOptions { Converters = { new JsonStringEnumConverter() } };
        var r = await _defaultClient.Rait<RaitEnumTestController>().Call(n => n.Get());
        Assert.That(r, Is.Not.Null);
    }
    
    [Test]
    public async Task FormModelNullTest()
    {
        var model = new ModelWithNullValues { Id = 10 };
        var responseModel = await _defaultClient.Rait<RaitTestController>().Call(n => n.FormModelNull(model));
        Assert.That(responseModel!.Id, Is.EqualTo(model.Id));
    }
}