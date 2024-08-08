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
       
        var call = await _defaultClient.Rait<RaitFilterTestController>().CallR(n =>
            n.GetReportsResult("fff", "qqq", "sss"));
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
        await _defaultClient.Rait<RaitActionResultTestController>().Call(n => n.Get());
        Assert.ThrowsAsync<RaitHttpException>(async () =>
            await _defaultClient.Rait<RaitActionResultTestController>().Call(n => n.Get401StatusCode()));

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

    [Test]
    public async Task DerivedRespTest()
    {
        var response = await _defaultClient.Rait<RaitDerivedResponseController>()
            .Call<ChildResp, BaseResp>(n => n.GetChildResp());
        Assert.That(response, Is.Not.Null);
    }

    [Test]
    public async Task RespWithoutDeserializationTest()
    {
        var response = await _defaultClient.Rait<RaitTestController>().CallWithoutDeserialization(n => n.Get());
        Assert.Multiple(() =>
        {
            Assert.That(response.GetType(), Is.EqualTo(typeof(string)));
            Assert.That(response, Is.Not.Null);
        });
    }

    [Test]
    public async Task GetWithGuidArrayTest()
    {
        var request = new ArrayValueRequest { Array = new List<Guid> { Guid.NewGuid(), Guid.NewGuid() } };
        var response = await _defaultClient.Rait<RaitTestController>().CallR(n => n.GetWithArray(request));
        Assert.That(response.Array, Is.Not.Empty);
    }

    [Test]
    public async Task GetWithDateTest()
    {
        var request = new DateTimeRequest { DateTime = DateTime.UtcNow };
        var response = await _defaultClient.Rait<RaitTestController>().CallR(n => n.GetWithDate(request));
        Assert.That(response.DateTime, Is.EqualTo(request.DateTime));
    }
}