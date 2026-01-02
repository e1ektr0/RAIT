using Microsoft.Extensions.DependencyInjection;

namespace RAIT.Core;

public static class ServiceCollectionExtensions
{
    // ReSharper disable once UnusedMethodReturnValue.Global
    public static IServiceCollection AddRait(this IServiceCollection services)
    {
        services.AddSingleton<SerializationConfigurationManager>();
        return services;
    }

    public static void ConfigureRait(this IServiceProvider serviceProvider, bool enableDocumentationGeneration = false)
    {
        if (enableDocumentationGeneration)
            RaitDocumentationConfig.Enable();
        var serializationConfigManager = serviceProvider.GetRequiredService<SerializationConfigurationManager>();
        serializationConfigManager.Configure();
        ServiceLocator.ServiceProvider = serviceProvider;
    }
}