using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using RAIT.Core;
using RAIT.Example.API.Endpoints.Endpoints.ActionResults;
using RAIT.Example.API.Endpoints.Endpoints.Simple;
using RAIT.Example.API.Endpoints.Endpoints.Simple.Models;

namespace RAIT.Example.API.Endpoints.Test;

public class SimpleEndpointsTests
{
    private WebApplicationFactory<Program> _application;
    private HttpClient _defaultClient;

    [SetUp]
    public void Setup()
    {
        _application = new WebApplicationFactory<Program>()
            .WithWebHostBuilder(PrepareEnv);

        _defaultClient = _application.CreateDefaultClient();
        
        RaitConfig.UseNewtonsoft();
    }

    private void PrepareEnv(IWebHostBuilder _)
    {
        _.UseEnvironment("Test");
    }

    [Test]
    public async Task SimpleCall()
    {
        await _defaultClient.Rait<SimpleEndpoint>()
            .CallR(n => n.HandleAsync(new CancellationToken()));
    }

    [Test]
    public async Task GetEndpointCall()
    {
        var actionResult = await _defaultClient.Rait<GetEndpoint>()
            .CallR(n => n.HandleAsync(new AggregatedGetRequest
            {
                ValueStr = "val",
                ExternalAccountId = "ext"
            }, new CancellationToken()));

        Assert.That(actionResult.Value!.ValueStr, Is.EqualTo("val"));
        Assert.That(actionResult.Value.ExternalAccountId, Is.EqualTo("ext"));
    }
    
    
    [Test]
    public async Task PostEndpointCall()
    {
        var req = new PostRequest
        {
            ExternalAccountId = "ddd",
            Origin =  new AggregatedGetRequest
            {
                ValueStr = "val",
                ExternalAccountId = "ext"
            }
        };
        await _defaultClient.Rait<PostEndpoint>()
            .CallR(n => n.HandleAsync( req, new CancellationToken()));

    }
}