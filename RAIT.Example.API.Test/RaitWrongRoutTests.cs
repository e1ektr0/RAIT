using RAIT.Core;
using RAIT.Example.API.Controllers;
using RAIT.Example.API.Test.Infrastructure;

namespace RAIT.Example.API.Test;

public sealed class RaitWrongRoutTests : RaitTestBase
{
    [Test]
    public async Task Get_WrongRout_NoIssue()
    {
        await Client.Rait<RaitWrongRoutController>().Call(n => n.Get());
    }
}
