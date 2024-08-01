using System.Net.Http.Json;
using System.Text.Json;
using RAIT.Core.DocumentationGenerator.XmlDoc;

namespace RAIT.Core;

public class RaitConfig
{
    public static JsonSerializerOptions? SerializationOptions { get; set; }
    public static XmlDoc DocState { get; set; }
    public static string? ResultPath { get; set; }

    public static bool DocGeneration = true;

    public static Func<HttpContent, Task<object?>>? DeserializeFunction { get; set; }

    public static Func<object?, JsonContent>? SerializeFunction { get; set; }
}