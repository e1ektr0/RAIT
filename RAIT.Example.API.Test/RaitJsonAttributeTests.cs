using RAIT.Core;
using RAIT.Example.API.Controllers;
using RAIT.Example.API.Models;
using RAIT.Example.API.Test.Infrastructure;

namespace RAIT.Example.API.Test;

public sealed class RaitJsonAttributeTests : RaitTestBase
{
    [Test]
    public async Task Ping_ValidAttributeModel_ReturnsExpectedResponse()
    {
        var model = new AttributeModel("test");
        var attributeResponseModel = await Client.Rait<RaitAttributesController>()
            .CallRequiredAsync(n => n.Ping(model));

        Assert.That(attributeResponseModel.Domain, Is.EqualTo(model.Id));
    }

    [Test]
    public async Task Ping_ValidAttributeModel_ReturnsSuccessStatusCode()
    {
        var model = new AttributeModel("test");
        var httpResponseMessage = await Client.Rait<RaitAttributesController>()
            .CallHttpAsync(n => n.Ping(model));

        httpResponseMessage.EnsureSuccessStatusCode();
    }
}
