using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using RAIT.Core;
using RAIT.Example.API.Controllers;
using RAIT.Example.API.Models;

namespace RAIT.Example.API.Test;

public sealed class RaitAttributeTests
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
    public async Task PingAttributeTest()
    {
        var model = new AttributeModel("test");
        var attributeResponseModel = await _defaultClient.Rait<RaitAttributesController>()
            .CallR(n => n.Ping(model));

        Assert.That(attributeResponseModel.Domain, Is.EqualTo(model.Id));
    }
    [Test]
    public async Task CallHTest()
    {
        var model = new AttributeModel("test");
        var httpResponseMessage = await _defaultClient.Rait<RaitAttributesController>()
            .CallH(n => n.Ping(model));

        httpResponseMessage.EnsureSuccessStatusCode();
    }
}