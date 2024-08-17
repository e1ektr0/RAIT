using Microsoft.AspNetCore.Mvc;

namespace RAIT.Core;

public static class RaitHttpClientExtensions
{
    public static RaitHttpClientWrapper<TController> Rait<TController>(this HttpClient client)
        where TController : ControllerBase
    {
        return new RaitHttpClientWrapper<TController>(client);
    }
}