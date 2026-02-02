using RAIT.Core;
using RAIT.Example.API.Controllers;
using RAIT.Example.API.Models;
using RAIT.Example.API.Test.Infrastructure;

namespace RAIT.Example.API.Test;

public sealed class RaitQueryModelTests : RaitTestBase
{
    [Test]
    public async Task Ping_ValidModel_ReturnsExpectedResponse()
    {
        var newGuid = Guid.NewGuid();
        var guid = Guid.NewGuid();
        var request = new Model
        {
            Id = 1,
            List = [newGuid, guid],
            Domain = "google.com",
            EnumList = [EnumExample.One, EnumExample.Three]
        };

        var response = await Client.Rait<RaitGetModelController>()
            .CallRequiredAsync(n => n.Ping(new Model
            {
                Id = 1,
                List = new List<Guid> { newGuid, guid },
                Domain = "google.com",
                EnumList = new List<EnumExample> { EnumExample.One, EnumExample.Three }
            }));

        Assert.That(response.Id, Is.EqualTo(request.Id));
        Assert.That(response.List![1], Is.EqualTo(request.List[1]));
        Assert.That(response.Domain, Is.EqualTo(request.Domain));
        Assert.That(response.EnumList!.First(), Is.EqualTo(EnumExample.One));
        Assert.That(response.EnumList!.Last(), Is.EqualTo(EnumExample.Three));
    }

    [Test]
    public void WrongModelType_InvalidModel_ThrowsRaitHttpException()
    {
        var request = new Model
        {
            Id = 1,
            List = [Guid.NewGuid(), Guid.NewGuid()]
        };

        Assert.ThrowsAsync<RaitHttpException>(async () =>
        {
            await Client.Rait<RaitGetModelController>()
                .CallRequiredAsync(n => n.WrongModelType(request));
        });
    }
}
