using RAIT.Core;
using RAIT.Example.API.Controllers;
using RAIT.Example.API.Test.Infrastructure;

namespace RAIT.Example.API.Test;

public sealed class RaitEmptyResponseTests : RaitTestBase
{
    [Test]
    public async Task Post_ValidId_ReturnsEmptyResponse()
    {
        await Client.Rait<RaitEmptyResponseController>().CallAsync(n => n.Post(10));
    }
}
