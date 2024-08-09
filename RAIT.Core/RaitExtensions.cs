using Microsoft.AspNetCore.Mvc;

namespace RAIT.Core;

public static class RaitExtensions
{
    public static RaitHttpWrapper<TController> Rait<TController>(this HttpClient client)
        where TController : ControllerBase
    {
        return new RaitHttpWrapper<TController>(client);
    }

}