using System.Text;
using System.Text.Json;
using Newtonsoft.Json;

namespace RAIT.Core;

public class RaitSerializationConfig
{
    public static JsonSerializerOptions? SerializationOptions { get; set; }
    internal static Func<HttpContent, Type, Task<object?>>? DeserializeFunction { get; set; }

    internal static Func<object?, HttpContent>? SerializeFunction { get; set; }

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
}