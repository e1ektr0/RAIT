using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;

namespace RAIT.Core;

public class SerializationConfigurationManager
{
    private readonly IActionResultExecutor<JsonResult> _jsonResultExecutor;
    private readonly IOptions<JsonOptions> _systemTextJsonOptions;
    private readonly IOptions<MvcNewtonsoftJsonOptions> _newtonsoftJsonOptions;

    public SerializationConfigurationManager(
        IActionResultExecutor<JsonResult> jsonResultExecutor,
        IOptions<JsonOptions> systemTextJsonOptions,
        IOptions<MvcNewtonsoftJsonOptions> newtonsoftJsonOptions)
    {
        _jsonResultExecutor = jsonResultExecutor;
        _systemTextJsonOptions = systemTextJsonOptions;
        _newtonsoftJsonOptions = newtonsoftJsonOptions;
    }

    public void Configure()
    {
        if (_jsonResultExecutor.GetType().Name.Contains("Newtonsoft"))
        {
            RaitSerializationConfig.UseNewtonsoft(_newtonsoftJsonOptions.Value.SerializerSettings);
        }
        else
        {
            RaitSerializationConfig.SerializationOptions = _systemTextJsonOptions.Value.JsonSerializerOptions;
        }
    }
}

public static class ServiceCollectionExtensions
{
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
    }
}

public static class RaitSerializationConfig
{
    internal static JsonSerializerOptions? SerializationOptions { get; set; }
    private static JsonSerializerSettings? SerializerSettings { get; set; }
    internal static Func<HttpContent, Type, Task<object?>>? DeserializeFunction { get; set; }
    internal static Func<object?, HttpContent>? SerializeFunction { get; set; }

    internal static void UseNewtonsoft(JsonSerializerSettings serializerSettings)
    {
        SerializerSettings = serializerSettings;
        SerializeFunction = SerializeWithNewtonsoft;
        DeserializeFunction = DeserializeWithNewtonsoftAsync;
    }

    private static async Task<object?> DeserializeWithNewtonsoftAsync(HttpContent content, Type type)
    {
        var jsonStr = await content.ReadAsStringAsync();
        return JsonConvert.DeserializeObject(jsonStr, type, SerializerSettings);
    }

    private static HttpContent SerializeWithNewtonsoft(object? obj)
    {
        var jsonStr = JsonConvert.SerializeObject(obj, SerializerSettings);
        return new StringContent(jsonStr, Encoding.UTF8, "application/json");
    }
}