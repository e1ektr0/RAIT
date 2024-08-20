using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using RAIT.Core;
using RAIT.Example.API.Endpoints.Endpoints.FromQuery;
using RAIT.Example.API.Endpoints.Endpoints.FromQuery.Models;
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
        _application.Services.ConfigureRait();
    }

    private void PrepareEnv(IWebHostBuilder _)
    {
        _.UseEnvironment("Test");
        _.ConfigureTestServices(s => s.AddRait());
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
                ExternalAccountId = "ext",
                Model = new FromInternalModel
                {
                    Test = "test"
                }
            }, new CancellationToken()));

        Assert.That(actionResult.Value!.ValueStr, Is.EqualTo("val"));
        Assert.That(actionResult.Value.ExternalAccountId, Is.EqualTo("ext"));
    }


    [Test]
    public async Task FromQueryCall()
    {
        var companyModel = new CompanyModel
        {
            Test = "test",
            Test2 = "test test test"
        };
        var operationResponse = new OperationRequest<CompanyModel>(
            companyModel, 1,false);
        var actionResult = await _defaultClient.Rait<FromQueryEndpoint>()
            .CallR(n => n.HandleAsync(operationResponse, new CancellationToken()));

        Assert.That(actionResult.Value!.ValueStr, Is.EqualTo("val"));
        Assert.That(actionResult.Value.ExternalAccountId, Is.EqualTo("ext"));
    }


    [Test]
    public async Task PostEndpointCall()
    {
        var req = new PostRequest
        {
            ExternalAccountId = "ddd",
            Origin = new AggregatedGetRequest
            {
                ValueStr = "val",
                ExternalAccountId = "ext",
                Date = new DateOnly(2000, 1, 1)
            }
        };
        await _defaultClient.Rait<PostEndpoint>()
            .CallR(n => n.HandleAsync(req, new CancellationToken()));
    }
}