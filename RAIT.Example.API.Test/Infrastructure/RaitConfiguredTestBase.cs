using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using RAIT.Core;

namespace RAIT.Example.API.Test.Infrastructure;

/// <summary>
/// Base class for tests that require RAIT DI configuration (AddRait and ConfigureRait).
/// Use this for tests involving typed results, custom serialization, or other features
/// that require full RAIT service registration.
/// </summary>
public abstract class RaitConfiguredTestBase : RaitTestBase
{
    public override void Setup()
    {
        base.Setup();
        Services.ConfigureRait();
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        base.ConfigureWebHost(builder);
        builder.ConfigureTestServices(services => services.AddRait());
    }
}
