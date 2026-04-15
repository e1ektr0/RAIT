using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.TestHost;
using RAIT.Core;

namespace RAIT.Example.API.Test.Infrastructure;

public abstract class RaitLegacyConfiguredTestBase : RaitTestBase
{
    public override void Setup()
    {
        base.Setup();
        Services.ConfigureRait();
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        base.ConfigureWebHost(builder);
        builder.ConfigureTestServices(services =>
            services.AddRait(options =>
                options.TypedResultFailureBehavior = RaitTypedResultFailureBehavior.ThrowRaitHttpException));
    }
}
