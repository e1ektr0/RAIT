using Microsoft.Extensions.DependencyInjection;

namespace RAIT.Core;

public static class ServiceCollectionExtensions
{
    // ReSharper disable once UnusedMethodReturnValue.Global
    public static IServiceCollection AddRait(this IServiceCollection services, Action<RaitOptions>? configure = null)
    {
        var options = new RaitOptions();
        configure?.Invoke(options);

        services.AddSingleton(options);
        services.AddSingleton<SerializationConfigurationManager>();
        return services;
    }

    public static void ConfigureRait(this IServiceProvider serviceProvider, bool enableDocumentationGeneration = false)
    {
        if (enableDocumentationGeneration)
            RaitDocumentationConfig.Enable();

        var options = serviceProvider.GetService<RaitOptions>();
        if (options != null)
            RaitGlobalConfiguration.TypedResultFailureBehavior = options.TypedResultFailureBehavior;

        var serializationConfigManager = serviceProvider.GetRequiredService<SerializationConfigurationManager>();
        serializationConfigManager.Configure();
        ServiceLocator.ServiceProvider = serviceProvider;
    }
}
