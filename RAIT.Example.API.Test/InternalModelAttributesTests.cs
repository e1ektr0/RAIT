using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using RAIT.Core;
using RAIT.Example.API.Controllers;
using RAIT.Example.API.Endpoints;
using RAIT.Example.API.Models;

namespace RAIT.Example.API.Test;

public sealed class InternalModelAttributesTests
{
    private WebApplicationFactory<Program> _application = null!;
    private HttpClient _defaultClient = null!;

    [SetUp]
    public void Setup()
    {

        ApiConfigurator.Configurate = builder => builder.AddNewtonsoftJson(options =>
        {
            options.SerializerSettings.Formatting = Formatting.Indented;
            options.SerializerSettings.ContractResolver = new CamelCasePropertyNamesContractResolver();
        });
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
            ExternalAccountId = "x",
            Model = new InternalModelInternalAttributes
            {
                Domain = "x2"
            }
        };
        var actionResult = await _defaultClient.Rait<InternalModelAttributesController>()
            .CallR(n => n.HttpPost(model));
    }
    
    
    
    //[Test]
    public async Task GetWithNewtonsoftAndEndpoint()
    {
        RaitConfig.UseNewtonsoft();
        var model = new InternalModelAttributes
        {
            ExternalAccountId = "x",
            Model = new InternalModelInternalAttributes
            {
                Domain = "x2"
            }
        };
        var actionResult = await _defaultClient.Rait<ExampleEndpoint>()
            .CallR(n => n.HandleAsync(model, new CancellationToken()));
        Assert.That(actionResult.Value!.ExternalId, Is.EqualTo("x"));
    }
}