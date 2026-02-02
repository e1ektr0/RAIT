using RAIT.Core;
using RAIT.Example.API.Controllers;
using RAIT.Example.API.Models;
using RAIT.Example.API.Test.Infrastructure;

namespace RAIT.Example.API.Test;

public sealed class RaitNewModelTests : RaitTestBase
{
    [Test]
    public async Task Post_ValidModel_ReturnsExpectedResult()
    {
        var model = new Model
        {
            Id = 10,
            Domain = "test",
            ExtraField = "extra"
        };

        var responseModel = await Client.Rait<RaitTestController>().Call(n => n.Post(model));

        Assert.That(responseModel!.Id, Is.EqualTo(10));
    }
}
