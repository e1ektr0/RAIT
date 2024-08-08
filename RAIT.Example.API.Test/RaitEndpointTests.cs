using Microsoft.AspNetCore.Hosting;
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

    //[Test]
    public async Task Get()
    {
        var model = new InternalModelAttributes
        {
            ExternalAccountId = "test",
            Model = new InternalModelInternalAttributes
            {
                Domain = "xxxx"
            }
        };
        var token = new CancellationToken();
        var actionResult = await _defaultClient.Rait<ExampleEndpoint>()
            .CallR(n => n.HandleAsync(model, token));

        Assert.That(actionResult.Value, Is.Not.Null);
        var valueId = actionResult.Value.Domain!;
        Assert.That(valueId, Is.EquivalentTo("xxxx"));
        Assert.That(actionResult.Value.ExternalId, Is.EquivalentTo("test"));
    }

}