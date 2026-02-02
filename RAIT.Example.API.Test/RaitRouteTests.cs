using RAIT.Core;
using RAIT.Example.API.Controllers;
using RAIT.Example.API.Test.Infrastructure;

namespace RAIT.Example.API.Test;

public sealed class RaitRouteTests : RaitTestBase
{
    [Test]
    public async Task Get_AbsoluteRoute_ReturnsExpectedResult()
    {
        await Client.Rait<RaitWrongRouteController>().CallAsync(n => n.Get());
    }
}
