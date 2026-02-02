using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using RAIT.Core;

namespace RAIT.Example.API.Endpoints.Test.Infrastructure;

/// <summary>
/// Base class for Endpoints API tests providing common WebApplicationFactory setup.
/// </summary>
public abstract class EndpointsTestBase
{
    private WebApplicationFactory<Program> _application = null!;
    protected HttpClient Client { get; private set; } = null!;

    [SetUp]
    public virtual void Setup()
    {
        _application = new WebApplicationFactory<Program>()
            .WithWebHostBuilder(ConfigureWebHost);
        Client = _application.CreateDefaultClient();
        _application.Services.ConfigureRait();
    }

    [TearDown]
    public virtual void TearDown()
    {
        Client.Dispose();
        _application.Dispose();
    }

    protected virtual void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Test");
        builder.ConfigureTestServices(services => services.AddRait());
    }
}
