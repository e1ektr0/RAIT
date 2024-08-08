using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using Newtonsoft.Json;
using RAIT.Core.DocumentationGenerator.XmlDoc;

namespace RAIT.Core;

public class RaitConfig
{
    public static JsonSerializerOptions? SerializationOptions { get; set; }
    public static XmlDoc DocState { get; set; }
    public static string? ResultPath { get; set; }

    public static bool DocGeneration = true;

    public static void UseNewtonsoft()
    {
        SerializeFunction = Serialize;
        DeserializeFunction = DeserializeAsync;
    }

    private static async Task<object?> DeserializeAsync(HttpContent content, Type type)
    {
        var jsonStr = await content.ReadAsStringAsync();
        var a = JsonConvert.DeserializeObject(jsonStr, type);
        return a;
    }

    private static HttpContent Serialize(object? obj)
    {
        var jsonStr = JsonConvert.SerializeObject(obj);
        return new StringContent(jsonStr, new UTF8Encoding(), "text/json");
    }

    public static Func<HttpContent, Type, Task<object?>>? DeserializeFunction { get; set; }

    public static Func<object?, HttpContent>? SerializeFunction { get; set; }
}