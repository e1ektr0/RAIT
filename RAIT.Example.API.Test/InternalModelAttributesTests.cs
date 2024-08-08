using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using RAIT.Core;
using RAIT.Example.API.Controllers;
using RAIT.Example.API.Models;

namespace RAIT.Example.API.Test;

public sealed class InternalModelAttributesTests
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
    public async Task Get()
    {
        var model = new InternalModelAttributes
        {
            ExternalAccountId = "x",
            Model = new Model
            {
                Domain = "x2"
            }
        };
        var actionResult = await _defaultClient.Rait<InternalModelAttributesController>()
            .CallR(n => n.HttpPost(model));
    }
}