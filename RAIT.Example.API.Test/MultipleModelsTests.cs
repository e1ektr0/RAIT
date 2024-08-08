using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using RAIT.Core;
using RAIT.Example.API.Controllers;
using RAIT.Example.API.Endpoints;
using RAIT.Example.API.Models;

namespace RAIT.Example.API.Test;

public sealed class MultipleModelsTests
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
        var model = new Model();
        var token = new CancellationToken();
        try
        {
            var actionResult = await _defaultClient.Rait<MultipleParametersTestController>()
                .CallR(n => n.Get(model, model));
        }
        catch (Exception e)
        {
            //just for debug
            Console.WriteLine(e.Message);
        }
    }
}