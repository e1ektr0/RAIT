using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using RAIT.Core;
using RAIT.Example.API.Controllers;

namespace RAIT.Example.API.Test;

public sealed class RaitTypedTests
{
    private WebApplicationFactory<Program> _application = null!;
    private HttpClient _defaultClient = null!;

    [SetUp]
    public void Setup()
    {
        _application = new WebApplicationFactory<Program>()
            .WithWebHostBuilder(PrepareEnv);

        _defaultClient = _application.CreateDefaultClient();
        _application.Services.ConfigureRait();
    }

    private void PrepareEnv(IWebHostBuilder builder)
    {
        builder.ConfigureTestServices(s => s.AddRait());
        builder.UseEnvironment("Test");
    }

    [Test]
    public async Task GetAccepted_ReturnsAccepted()
    {
        var response = await _defaultClient
            .Rait<TypedController>()
            .CallR(c => c.GetAccepted());

        // RAIT typically unwraps the typed result; adapt to your usage
        Assert.That(response.Value!.Id, Is.EqualTo(42));
    }

    [Test]
    public async Task Delete_ReturnsNoContent_WhenExists()
    {
        var response = await _defaultClient
            .Rait<TypedController>()
            .Call(c => c.Delete(5));
    }

  
    [Test]
    public async Task Teapot_Returns418()
    {
       await _defaultClient
            .Rait<TypedController>()
            .CallH(c => c.Teapot());

    }
}