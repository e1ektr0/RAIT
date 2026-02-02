using RAIT.Core;
using RAIT.Example.API.Controllers;
using RAIT.Example.API.Test.Infrastructure;

namespace RAIT.Example.API.Test;

public sealed class RaitPrimitiveTypeTests : RaitTestBase
{
    [Test]
    public async Task GetInt_WhenCalled_ReturnsInteger()
    {
        var result = await Client.Rait<RaitPrimitiveTypesTestController>().CallAsync(n => n.GetInt());
        Assert.That(result, Is.EqualTo(1));
    }

    [Test]
    public async Task GetString_WhenCalled_ReturnsString()
    {
        var call = await Client.Rait<RaitPrimitiveTypesTestController>().CallAsync(n => n.GetString());
        Assert.That(call, Is.EqualTo("test"));
    }

    [Test]
    public async Task GetGuid_WhenCalled_ReturnsGuid()
    {
        var result = await Client.Rait<RaitPrimitiveTypesTestController>().CallAsync(n => n.GetGuid());
        Assert.That(result, Is.EqualTo(Guid.Parse("6ec3e17e-c51c-43f0-b5d0-02889912a78c")));
    }

    [Test]
    public async Task GetBool_WhenCalled_ReturnsBoolean()
    {
        var result = await Client.Rait<RaitPrimitiveTypesTestController>().CallAsync(n => n.GetBool());
        Assert.That(result, Is.EqualTo(true));
    }

    [Test]
    public async Task GetObject_WhenCalled_ReturnsObject()
    {
        var result = await Client.Rait<RaitPrimitiveTypesTestController>().CallAsync(n => n.GetObject());
        Assert.That(result, Is.EqualTo("test"));
    }
}
