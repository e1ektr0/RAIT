using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;

namespace RAIT.Core;

internal class SerializationConfigurationManager
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


    internal void Configure()
    {
        if (_jsonResultExecutor.GetType().Name.Contains("Newtonsoft"))
        {
            RaitSerializationConfig.UseNewtonsoft(_newtonsoftJsonOptions.Value.SerializerSettings);
            RaitSerializationConfig.DateTimeOffsetToQuery = dto =>
                Newtonsoft.Json.JsonConvert.SerializeObject(dto, _newtonsoftJsonOptions.Value.SerializerSettings).Trim('"');
            RaitSerializationConfig.DateTimeToQuery = dt =>
                Newtonsoft.Json.JsonConvert.SerializeObject(dt, _newtonsoftJsonOptions.Value.SerializerSettings).Trim('"');
        }
        else
        {
            RaitSerializationConfig.SerializationOptions = _systemTextJsonOptions.Value.JsonSerializerOptions;
            RaitSerializationConfig.DateTimeOffsetToQuery = dto =>
                System.Text.Json.JsonSerializer.Serialize(dto, _systemTextJsonOptions.Value.JsonSerializerOptions).Trim('"');
            RaitSerializationConfig.DateTimeToQuery = dt =>
                System.Text.Json.JsonSerializer.Serialize(dt, _systemTextJsonOptions.Value.JsonSerializerOptions).Trim('"');
        }
    }
}

internal static class RaitSerializationConfig
{
    internal static JsonSerializerOptions? SerializationOptions { get; set; }
    private static JsonSerializerSettings? SerializerSettings { get; set; }
    internal static Func<HttpContent, Type, Task<object?>>? DeserializeFunction { get; set; }
    internal static Func<object?, HttpContent>? SerializeFunction { get; set; }

    public static Func<DateTimeOffset, string> DateTimeOffsetToQuery { get; internal set; } =
        dto => dto.ToString("O");
    public static Func<DateTime, string> DateTimeToQuery { get; internal set; } =
        dt => dt.ToString("O");
    
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