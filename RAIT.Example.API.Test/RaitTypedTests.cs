using RAIT.Core;
using RAIT.Example.API.Controllers;
using RAIT.Example.API.Test.Infrastructure;

namespace RAIT.Example.API.Test;

public sealed class RaitTypedTests : RaitConfiguredTestBase
{
    [Test]
    public async Task GetAccepted_ReturnsAccepted()
    {
        var response = await Client
            .Rait<TypedController>()
            .CallR(c => c.GetAccepted());

        Assert.That(response.Value!.Id, Is.EqualTo(42));
    }

    [Test]
    public async Task Delete_ReturnsNoContent_WhenExists()
    {
        await Client
            .Rait<TypedController>()
            .Call(c => c.Delete(5));
    }

    [Test]
    public async Task Teapot_Returns418()
    {
        await Client
            .Rait<TypedController>()
            .CallH(c => c.Teapot());
    }
}
