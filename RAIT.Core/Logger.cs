using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace RAIT.Core;

public class RaitLogger
{
    public static void Log(string message, List<string> files)
    {
        if (ServiceLocator.ServiceProvider == null)
            return;

        var service = ServiceLocator.ServiceProvider.GetService<ILogger<RaitLogger>>();
        if (service != null)
            service.LogDebug($"{message} {string.Join(",", files)} ");
    }
}