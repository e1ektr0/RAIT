using RAIT.Core;
using RAIT.Example.API.Endpoints.Endpoints.FromQuery;
using RAIT.Example.API.Endpoints.Endpoints.FromQuery.Models;
using RAIT.Example.API.Endpoints.Endpoints.Simple;
using RAIT.Example.API.Endpoints.Endpoints.Simple.Models;
using RAIT.Example.API.Endpoints.Test.Infrastructure;

namespace RAIT.Example.API.Endpoints.Test;

public sealed class SimpleEndpointsTests : EndpointsTestBase
{
    [Test]
    public async Task SimpleCall()
    {
        await Client.Rait<SimpleEndpoint>()
            .CallR(n => n.HandleAsync(CancellationToken.None));
    }

    [Test]
    public async Task GetEndpointCall()
    {
        var actionResult = await Client.Rait<GetEndpoint>()
            .CallR(n => n.HandleAsync(new AggregatedGetRequest
            {
                ValueStr = "https://google.com",
                ExternalAccountId = "ext",
                Model = new FromInternalModel { Test = "test" },
                HeaderTest = "header"
            }, CancellationToken.None));

        Assert.That(actionResult.Value!.ValueStr, Is.EqualTo("https://google.com"));
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
        var operationResponse = new OperationRequest<CompanyModel>(companyModel, 1, false);
        var actionResult = await Client.Rait<FromQueryEndpoint>()
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
                ValueStr = "https://google.com",
                ExternalAccountId = "ext",
                Date = new DateOnly(2000, 1, 1)
            },
            Test = "yyy"
        };

        await Client.Rait<PostEndpoint>()
            .CallR(n => n.HandleAsync(req, new CancellationToken()));
    }

    [Test]
    public async Task PostFromFormEndpointCall()
    {
        var req = new PostFromFormRequest
        {
            Param1 = "param1",
            Param2 = "param2"
        };

        await Client.Rait<PostFromFormEndpoint>()
            .CallR(n => n.HandleAsync(req, new CancellationToken()));
    }
}
