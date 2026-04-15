using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using RAIT.Core;

namespace RAIT.Example.API.Test.Infrastructure;

/// <summary>
/// Base class for all RAIT tests providing common WebApplicationFactory setup and HttpClient creation.
/// </summary>
public abstract class RaitTestBase
{
    private WebApplicationFactory<Program> _application = null!;
    protected HttpClient Client { get; private set; } = null!;
    protected IServiceProvider Services => _application.Services;

    [SetUp]
    public virtual void Setup()
    {
        RaitGlobalConfiguration.TypedResultFailureBehavior = RaitTypedResultFailureBehavior.ReturnTypedResult;
        _application = new WebApplicationFactory<Program>()
            .WithWebHostBuilder(ConfigureWebHost);
        Client = _application.CreateDefaultClient();
    }

    [TearDown]
    public virtual void TearDown()
    {
        RaitGlobalConfiguration.TypedResultFailureBehavior = RaitTypedResultFailureBehavior.ReturnTypedResult;
        Client.Dispose();
        _application.Dispose();
    }

    protected virtual void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Test");
    }
}
