using RAIT.Core;
using RAIT.Example.API.Controllers;
using RAIT.Example.API.Models;
using RAIT.Example.API.Test.Infrastructure;

namespace RAIT.Example.API.Test;

public sealed class RaitFileTests : RaitTestBase
{
    [Test]
    public async Task Post_FileModel_ReturnsExpectedResult()
    {
        using var model = new RaitFormFile("example.txt", "image/png");
        var responseModel = await Client.Rait<RaitTestFileController>().Call(n => n.Post(model));

        Assert.That(responseModel!.Id, Is.EqualTo(10));
    }

    [Test]
    public async Task Post3_ModelAndFile_ReturnsExpectedResult()
    {
        using var file = new RaitFormFile("example.txt", "image/png");
        var model = new Model { Id = 10, List = new List<Guid> { Guid.NewGuid() } };
        var responseModel = await Client.Rait<RaitTestFileController>().Call(n => n.Post3(model, file));

        Assert.That(responseModel!.Id, Is.EqualTo(model.Id));
    }

    [Test]
    public async Task Post_FileWithContent_ReturnsExpectedResult()
    {
        using var model = new RaitFormFile("example.txt", "image/png", await File.ReadAllBytesAsync("example2.txt"));
        var responseModel = await Client.Rait<RaitTestFileController>().Call(n => n.Post(model));

        Assert.That(responseModel!.Id, Is.EqualTo(11));
    }
}
